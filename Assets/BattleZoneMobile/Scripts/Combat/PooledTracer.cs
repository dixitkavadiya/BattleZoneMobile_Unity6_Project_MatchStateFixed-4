using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleZoneMobile
{
    [RequireComponent(typeof(LineRenderer))]
    public class PooledTracer : MonoBehaviour
    {
        private const int MaxPoolSize = 64;
        private static readonly Stack<PooledTracer> Pool = new Stack<PooledTracer>(MaxPoolSize);

        private LineRenderer line;
        private float remainingLife;

        public LineRenderer Line
        {
            get
            {
                if (line == null)
                {
                    line = GetComponent<LineRenderer>();
                }

                return line;
            }
        }

        public static PooledTracer Get()
        {
            PooledTracer tracer = Pool.Count > 0 ? Pool.Pop() : Create();
            tracer.gameObject.SetActive(true);
            tracer.enabled = true;
            return tracer;
        }

        public void Play(float lifeTime)
        {
            remainingLife = Mathf.Max(0.01f, lifeTime);
        }

        private void Update()
        {
            remainingLife -= Time.deltaTime;
            if (remainingLife > 0f)
            {
                return;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            enabled = false;
            gameObject.SetActive(false);

            if (Pool.Count < MaxPoolSize)
            {
                Pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private static PooledTracer Create()
        {
            GameObject tracerObject = new GameObject("Runtime Bullet Tracer");
            LineRenderer renderer = tracerObject.AddComponent<LineRenderer>();
            renderer.useWorldSpace = true;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.numCapVertices = 0;
            renderer.numCornerVertices = 0;
            renderer.receiveShadows = false;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            PooledTracer tracer = tracerObject.AddComponent<PooledTracer>();
            tracer.line = renderer;
            return tracer;
        }
    }
}
