using System.Collections.Generic;
using UnityEngine;

namespace BattleZoneMobile
{
    public class PooledShellCasing : MonoBehaviour
    {
        private const int MaxPoolSize = 48;
        private static readonly Stack<PooledShellCasing> Pool = new Stack<PooledShellCasing>(MaxPoolSize);
        private static Material shellMaterial;

        private Vector3 velocity;
        private Vector3 angularVelocity;
        private float remainingLife;

        public static void Spawn(Vector3 position, Quaternion rotation, Vector3 ejectDirection)
        {
            PooledShellCasing shell = Pool.Count > 0 ? Pool.Pop() : Create();
            shell.transform.SetPositionAndRotation(position, rotation);
            shell.velocity = ejectDirection.normalized * Random.Range(1.3f, 2.2f) + Vector3.up * Random.Range(0.55f, 0.95f);
            shell.angularVelocity = new Vector3(Random.Range(260f, 520f), Random.Range(-380f, 380f), Random.Range(-240f, 240f));
            shell.remainingLife = 0.72f;
            shell.gameObject.SetActive(true);
            shell.enabled = true;
        }

        private void Update()
        {
            remainingLife -= Time.deltaTime;
            velocity += Physics.gravity * 0.62f * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(angularVelocity * Time.deltaTime, Space.Self);

            if (remainingLife <= 0f)
            {
                ReturnToPool();
            }
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

        private static PooledShellCasing Create()
        {
            GameObject shellObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shellObject.name = "Runtime Shell Casing";
            shellObject.transform.localScale = new Vector3(0.035f, 0.08f, 0.035f);

            Collider collider = shellObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = shellObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = GetMaterial();
            }

            PooledShellCasing shell = shellObject.AddComponent<PooledShellCasing>();
            shell.enabled = false;
            shellObject.SetActive(false);
            return shell;
        }

        private static Material GetMaterial()
        {
            if (shellMaterial != null)
            {
                return shellMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            shellMaterial = new Material(shader)
            {
                name = "Runtime Shell Brass"
            };

            if (shellMaterial.HasProperty("_BaseColor"))
            {
                shellMaterial.SetColor("_BaseColor", new Color(0.82f, 0.58f, 0.22f, 1f));
            }

            if (shellMaterial.HasProperty("_Color"))
            {
                shellMaterial.SetColor("_Color", new Color(0.82f, 0.58f, 0.22f, 1f));
            }

            return shellMaterial;
        }
    }
}
