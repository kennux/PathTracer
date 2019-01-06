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

            private Bucket[] children;
            private BoundingBoxSoA childrenBounds;

            public bool isSubdivided;
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
                BoundingBox _frontBottomLeft, _frontBottomRight, _frontTopLeft, _frontTopRight, _backBottomLeft, _backBottomRight, _backTopLeft, _backTopRight;
                BoundingBox.Subdivide(this.boundingBox, out _frontBottomLeft, out _frontBottomRight, out _frontTopLeft,
                    out _frontTopRight, out _backBottomLeft, out _backBottomRight, out _backTopLeft, out _backTopRight);

                this.isSubdivided = true;
                Bucket frontBottomLeft = new Bucket(this.tree, _frontBottomLeft, this);
                Bucket frontBottomRight = new Bucket(this.tree, _frontBottomRight, this);
                Bucket frontTopLeft = new Bucket(this.tree, _frontTopLeft, this);
                Bucket frontTopRight = new Bucket(this.tree, _frontTopRight, this);
                Bucket backBottomLeft = new Bucket(this.tree, _backBottomLeft, this);
                Bucket backBottomRight = new Bucket(this.tree, _backBottomRight, this);
                Bucket backTopLeft = new Bucket(this.tree, _backTopLeft, this);
                Bucket backTopRight = new Bucket(this.tree, _backTopRight, this);

                frontBottomLeft.BuildFromParent();
                frontBottomRight.BuildFromParent();
                frontTopLeft.BuildFromParent();
                frontTopRight.BuildFromParent();
                backBottomLeft.BuildFromParent();
                backBottomRight.BuildFromParent();
                backTopLeft.BuildFromParent();
                backTopRight.BuildFromParent();
                this.children = new Bucket[8];
                this.children[0] = frontBottomLeft;
                this.children[1] = frontBottomRight;
                this.children[2] = frontTopLeft;
                this.children[3] = frontTopRight;
                this.children[4] = backBottomLeft;
                this.children[5] = backBottomRight;
                this.children[6] = backTopLeft;
                this.children[7] = backTopRight;
                this.childrenBounds = new BoundingBoxSoA(8);
                this.childrenBounds[0] = _frontBottomLeft;
                this.childrenBounds[1] = _frontBottomRight;
                this.childrenBounds[2] = _frontTopLeft;
                this.childrenBounds[3] = _frontTopRight;
                this.childrenBounds[4] = _backBottomLeft;
                this.childrenBounds[5] = _backBottomRight;
                this.childrenBounds[6] = _backTopLeft;
                this.childrenBounds[7] = _backTopRight;

                this.container = default(T); // Reset container
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

                PropagateRaycast<TState>(ray, objectCallback, ref stateData);
            }

            private void PropagateRaycast<TState>(Ray ray, RaycastCallback<TState> objectCallback, ref TState stateData)
            {
                if (!this.isSubdivided)
                {
                    objectCallback(ref stateData, ref this.container);
                    return;
                }

                // Propagate
                Vector3[] min = this.childrenBounds.min;
                Vector3[] max = this.childrenBounds.max;
                Vector3[] center = this.childrenBounds.center;
                for (int i = 0; i < 8; i++)
                {
                    if (BoundingBox.RayIntersection(ref min[i], ref max[i], ref center[i], ref ray))
                        this.children[i].PropagateRaycast<TState>(ray, objectCallback, ref stateData);
                }
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
