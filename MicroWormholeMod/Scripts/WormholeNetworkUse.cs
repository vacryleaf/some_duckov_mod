using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞网络使用行为
    /// 放置两个配对的传送门
    /// </summary>
    public class WormholeNetworkUse : MonoBehaviour
    {
        [Header("网络设置")]
        public float portalDuration = 60f;
        public float teleportCooldown = 5f;
        public float teleportDelay = 1f;
        public int maxNetworks = 3;

        private CharacterMainControl character;
        private static List<WormholeNetwork> activeNetworks = new List<WormholeNetwork>();

        void Start()
        {
            character = GetComponentInParent<CharacterMainControl>();
        }

        public bool CanPlaceNetwork()
        {
            return activeNetworks.Count < maxNetworks;
        }

        public bool PlaceNetwork()
        {
            if (!CanPlaceNetwork()) return false;
            if (character == null) return false;

            Vector3 placePosition = character.transform.position + character.transform.forward * 2f;
            placePosition.y = character.transform.position.y;

            WormholeNetwork network = WormholeNetwork.Create(character.gameObject, placePosition, portalDuration, teleportCooldown, teleportDelay);

            if (network != null)
            {
                activeNetworks.Add(network);
                ModLogger.Log(string.Format("[虫洞网络] 已放置网络，有效时间: {0}秒", portalDuration));
                return true;
            }

            return false;
        }

        public static void RemoveNetwork(WormholeNetwork network)
        {
            if (activeNetworks.Contains(network))
            {
                activeNetworks.Remove(network);
            }
        }

        public static void ClearAllNetworks()
        {
            foreach (var network in activeNetworks)
            {
                if (network != null)
                {
                    network.Destroy();
                }
            }
            activeNetworks.Clear();
        }
    }

    public class WormholeNetwork
    {
        public class WormholePortalA : MonoBehaviour
        {
            private WormholeNetwork network;
            private float cooldownTimer = 0f;

            public void Initialize(WormholeNetwork parentNetwork)
            {
                network = parentNetwork;

                // 蓝色粒子
                ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = new Color(0f, 0.5f, 1f, 0.8f);
                main.startSize = 0.3f;
                main.startSpeed = 2f;
                main.startLifetime = 1.5f;
                main.maxParticles = 100;

                var emission = ps.emission;
                emission.rateOverTime = 30;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 1.5f;

                SphereCollider collider = gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 1.5f;

                gameObject.AddComponent<RotateAnimation>();
            }

            void Update()
            {
                if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
            }

            void OnTriggerEnter(Collider other)
            {
                if (cooldownTimer > 0) return;
                if (network == null || network.portalB == null) return;

                CharacterMainControl character = other.GetComponentInParent<CharacterMainControl>();
                if (character == null) return;

                StartCoroutine(TeleportToB(character));
            }

            IEnumerator TeleportToB(CharacterMainControl character)
            {
                cooldownTimer = network.teleportCooldown;
                yield return new WaitForSeconds(network.teleportDelay);

                Vector3 targetPosition = network.portalB.transform.position + Vector3.up * 0.5f;
                character.transform.position = targetPosition;
                PlayArrivalEffect();

                ModLogger.Log(string.Format("[虫洞网络] {0} 从A传送到B", character.name));
            }

            void PlayArrivalEffect()
            {
                GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                effect.transform.position = network.portalB.transform.position;
                effect.transform.localScale = Vector3.one * 2f;
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.5f, 0f, 0.5f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;
                effect.GetComponent<Renderer>().material = mat;
                Destroy(effect, 0.5f);
            }
        }

        public class WormholePortalB : MonoBehaviour
        {
            private WormholeNetwork network;
            private float cooldownTimer = 0f;

            public void Initialize(WormholeNetwork parentNetwork)
            {
                network = parentNetwork;

                ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = new Color(1f, 0.5f, 0f, 0.8f);
                main.startSize = 0.3f;
                main.startSpeed = 2f;
                main.startLifetime = 1.5f;
                main.maxParticles = 100;
                var emission = ps.emission;
                emission.rateOverTime = 30;
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 1.5f;

                SphereCollider collider = gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 1.5f;

                gameObject.AddComponent<RotateAnimation>();
            }

            void Update()
            {
                if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
            }

            void OnTriggerEnter(Collider other)
            {
                if (cooldownTimer > 0) return;
                if (network == null || network.portalA == null) return;

                CharacterMainControl character = other.GetComponentInParent<CharacterMainControl>();
                if (character == null) return;

                StartCoroutine(TeleportToA(character));
            }

            IEnumerator TeleportToA(CharacterMainControl character)
            {
                cooldownTimer = network.teleportCooldown;
                yield return new WaitForSeconds(network.teleportDelay);

                Vector3 targetPosition = network.portalA.transform.position + Vector3.up * 0.5f;
                character.transform.position = targetPosition;
                PlayArrivalEffect();

                ModLogger.Log(string.Format("[虫洞网络] {0} 从B传送到A", character.name));
            }

            void PlayArrivalEffect()
            {
                GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                effect.transform.position = network.portalA.transform.position;
                effect.transform.localScale = Vector3.one * 2f;
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0f, 0.5f, 1f, 0.5f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;
                effect.GetComponent<Renderer>().material = mat;
                Destroy(effect, 0.5f);
            }
        }

        public GameObject portalA;
        public GameObject portalB;
        public float portalDuration;
        public float teleportCooldown;
        public float teleportDelay;

        private WormholeNetwork() { }

        public static WormholeNetwork Create(GameObject owner, Vector3 position, float duration, float cooldown, float delay)
        {
            WormholeNetwork network = new WormholeNetwork();
            network.portalDuration = duration;
            network.teleportCooldown = cooldown;
            network.teleportDelay = delay;

            network.portalA = new GameObject("WormholePortal_A");
            network.portalA.transform.position = position;
            network.portalA.transform.rotation = Quaternion.identity;
            network.portalA.AddComponent<WormholePortalA>().Initialize(network);

            Vector3 bDirection = (owner.transform.forward + Vector3.up * 0.2f).normalized;
            Vector3 bPosition = position + bDirection * 30f;

            RaycastHit hit;
            if (Physics.Raycast(bPosition + Vector3.up * 50f, Vector3.down, out hit, 100f))
            {
                bPosition = hit.point + Vector3.up * 1f;
            }

            network.portalB = new GameObject("WormholePortal_B");
            network.portalB.transform.position = bPosition;
            network.portalB.transform.rotation = Quaternion.identity;
            network.portalB.AddComponent<WormholePortalB>().Initialize(network);

            // 使用MonoBehavior启动协程
            var runner = network.portalA.AddComponent<CoroutineRunner>();
            runner.StartCoroutine(network.DurationCountdown());

            ModLogger.Log(string.Format("[虫洞网络] A门位置: {0}, B门位置: {1}",
                network.portalA.transform.position, network.portalB.transform.position));

            return network;
        }

        private IEnumerator DurationCountdown()
        {
            yield return new WaitForSeconds(portalDuration);
            Destroy();
        }

        public void Destroy()
        {
            if (portalA != null)
            {
                UnityEngine.Object.Destroy(portalA);
                portalA = null;
            }

            if (portalB != null)
            {
                UnityEngine.Object.Destroy(portalB);
                portalB = null;
            }

            WormholeNetworkUse.RemoveNetwork(this);
        }
    }

    public class RotateAnimation : MonoBehaviour
    {
        public Vector3 rotationSpeed = Vector3.zero;
        void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }

    public class CoroutineRunner : MonoBehaviour { }
}
