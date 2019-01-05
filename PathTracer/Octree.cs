using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public interface ISoAContainer<T> where T : ISoAContainer<T>
    {
        void CreateFromSubsetOfOther(T other, int[] indices);
    }

    public class OCTree<T> where T : ISoAContainer<T>, new()
    {
        public class Bucket
        {
            public BoundingBox boundingBox;
            public int[] indices;
            public T container;

            private Bucket parent;
            private OCTree<T> tree;

            public bool isSubdivided;
            public Bucket frontBottomLeft; // Min
            public Bucket frontBottomRight;
            public Bucket frontTopLeft;
            public Bucket frontTopRight;
            public Bucket backBottomLeft;
            public Bucket backBottomRight;
            public Bucket backTopLeft;
            public Bucket backTopRight; // Max
            public int depth = 0;

            public Bucket(OCTree<T> tree, BoundingBox boundingBox, Bucket parent = null)
            {
                this.tree = tree;
                this.boundingBox = boundingBox;
                this.parent = parent;
            }

            public void Subdivide()
            {
                if (this.isSubdivided || this.indices.Length <= this.tree.maxObjectsPerBucket || this.depth >= this.tree.maxDepth)
                    return;

                // Subdivide
                BoundingBox frontBottomLeft, frontBottomRight, frontTopLeft, frontTopRight, backBottomLeft, backBottomRight, backTopLeft, backTopRight;
                BoundingBox.Subdivide(this.boundingBox, out frontBottomLeft, out frontBottomRight, out frontTopLeft,
                    out frontTopRight, out backBottomLeft, out backBottomRight, out backTopLeft, out backTopRight);

                this.isSubdivided = true;
                this.frontBottomLeft = new Bucket(this.tree, frontBottomLeft, this);
                this.frontBottomRight = new Bucket(this.tree, frontBottomRight, this);
                this.frontTopLeft = new Bucket(this.tree, frontTopLeft, this);
                this.frontTopRight = new Bucket(this.tree, frontTopRight, this);
                this.backBottomLeft = new Bucket(this.tree, backBottomLeft, this);
                this.backBottomRight = new Bucket(this.tree, backBottomRight, this);
                this.backTopLeft = new Bucket(this.tree, backTopLeft, this);
                this.backTopRight = new Bucket(this.tree, backTopRight, this);

                this.frontBottomLeft.BuildFromParent();
                this.frontBottomRight.BuildFromParent();
                this.frontTopLeft.BuildFromParent();
                this.frontTopRight.BuildFromParent();
                this.backBottomLeft.BuildFromParent();
                this.backBottomRight.BuildFromParent();
                this.backTopLeft.BuildFromParent();
                this.backTopRight.BuildFromParent();

                int c = this.frontBottomLeft.indices.Length +
                    this.frontBottomRight.indices.Length +
                    this.frontTopLeft.indices.Length +
                    this.frontTopRight.indices.Length +
                    this.backBottomLeft.indices.Length +
                    this.backBottomRight.indices.Length +
                    this.backTopLeft.indices.Length +
                    this.backTopRight.indices.Length;
            }

            private static List<int> tmpList = new List<int>();
            private void BuildFromParent()
            {
                tmpList.Clear();
                int[] indices = this.parent.indices;
                int l = indices.Length;

                for (int i = 0; i < l; i++)
                {
                    int idx = indices[i];
                    if (this.boundingBox.Overlaps(tree.boundingBoxes[idx]))
                        tmpList.Add(idx);
                }

                this.indices = tmpList.ToArray();
                this.depth = this.parent.depth + 1;
                this.container = new T();
                this.container.CreateFromSubsetOfOther(this.tree.root.container, this.indices);
                Subdivide();
            }

            public void Raycast<TState>(Ray ray, RaycastCallback<TState> objectCallback, ref TState stateData)
            {
                if (!this.boundingBox.RayIntersection(ref ray))
                    return;

                if (!this.isSubdivided)
                {
                    objectCallback(ref stateData, ref this.container);
                    return;
                }

                // Propagate
                this.frontBottomLeft.Raycast(ray, objectCallback, ref stateData);
                this.frontBottomRight.Raycast(ray, objectCallback, ref stateData);
                this.frontTopLeft.Raycast(ray, objectCallback, ref stateData);
                this.frontTopRight.Raycast(ray, objectCallback, ref stateData);
                this.backBottomLeft.Raycast(ray, objectCallback, ref stateData);
                this.backBottomRight.Raycast(ray, objectCallback, ref stateData);
                this.backTopLeft.Raycast(ray, objectCallback, ref stateData);
                this.backTopRight.Raycast(ray, objectCallback, ref stateData);
            }
        }

        public delegate void RaycastCallback<TState>(ref TState state, ref T stateData);

        private int maxObjectsPerBucket;
        private int maxDepth;
        private Bucket root;
        private BoundingBox[] boundingBoxes;

        public void Raycast<TState>(Ray ray, RaycastCallback<TState> objectCallback, ref TState data)
        {
            root.Raycast(ray, objectCallback, ref data);
        }

        public void Bake(BoundingBox[] boundingBoxes, T baseContainer, int maxObjectsPerBucket = 32, int maxDepth = 16)
        {
            this.boundingBoxes = boundingBoxes;
            this.maxObjectsPerBucket = maxObjectsPerBucket;
            this.maxDepth = maxDepth;

            // Merge bounding box
            int[] indices = new int[boundingBoxes.Length];
            Vector3 min = Vector3.Zero, max = Vector3.Zero;
            for (int i = 0; i < boundingBoxes.Length; i++)
            {
                var bb = boundingBoxes[i];
                min = Vector3.Min(bb.min, min);
                max = Vector3.Max(bb.max, max);
                indices[i] = i;
            }

            BoundingBox bounds = new BoundingBox()
            {
                min = min,
                max = max
            };
            this.root = new Bucket(this, bounds);
            root.container = baseContainer;

            // Root bucket
            root.boundingBox = bounds;
            root.indices = indices;
            root.Subdivide();
        }
    }
}
