using UnityEngine;
using System.Collections;

namespace CameraUtilities
{
    [RequireComponent(typeof(Camera))]
    public class CameraZoom : MonoBehaviour
    {
        public Transform zoomIn;
        public Transform zoomOut;

        private Camera camera;

        private float orthographicSize = 5f;

        // Use this for initialization
        void Start()
        {
            camera = this.gameObject.GetComponent<Camera>();
            fov = camera.fieldOfView;
            aspect = camera.aspect;
            near = camera.nearClipPlane;
            far = camera.farClipPlane;
            aspect = (float)Screen.width / (float)Screen.height;
            ortho = Matrix4x4.Ortho(-1f* orthographicSize*aspect, 1f* orthographicSize* aspect, -1f * orthographicSize, 1f * orthographicSize, near, far);
            // perspective = camera.projectionMatrix; <-- somehow this doesn't work quite right
            perspective = Matrix4x4.Perspective(fov, aspect, near, far);
            camera.projectionMatrix = perspective;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateZoom();
            transform.position = Vector3.Lerp(zoomOut.position, zoomIn.position, cameraDistance);
            transform.rotation = Quaternion.Lerp(zoomOut.rotation, zoomIn.rotation, cameraDistance);
        }

        float scrollVelocity = 0f;
        float lastScroll = 0f;
        float cameraDistance = 1f; // between 0 and 1 --> 1 = zoom in, 0 = zoom out
        float cameraSpeed = 30f;
        void UpdateZoom()
        {
            var scroll = Input.GetAxis("LookZoom") * cameraSpeed * Time.deltaTime;
            scroll = Mathf.SmoothDamp(lastScroll, scroll, ref scrollVelocity, 0.1f);

            if (scroll > 0 && cameraDistance < 1f)
            {
                if (cameraDistance <= 0f)
                {
                    BlendToMatrix(perspective, 0.5f);
                    EventManager.TriggerEvent("Orthographic Mode", false);
                }
                cameraDistance += scroll;
            }
            if (scroll < 0 && cameraDistance > 0f)
            {
                cameraDistance += scroll;
                if (cameraDistance <= 0f)
                {
                    BlendToMatrix(ortho, 0.5f);
                    EventManager.TriggerEvent("Orthographic Mode", true);
                }
            }

            cameraDistance = Mathf.Clamp(cameraDistance, 0f, 1f);

            lastScroll = scroll;
        }

        // -- Switching zoom type -- //
        private Matrix4x4 ortho,
                        perspective;
        private float fov = 60f,
                     near = .3f,
                     far = 1000f;
        private float aspect;
        private bool orthoOn;

        public static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
        {
            Matrix4x4 ret = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                ret[i] = Mathf.Lerp(from[i], to[i], time);
            return ret;
        }

        private IEnumerator LerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                camera.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
                yield return 1;
            }
            camera.projectionMatrix = dest;
        }

        public Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration)
        {
            StopAllCoroutines();
            return StartCoroutine(LerpFromTo(camera.projectionMatrix, targetMatrix, duration));
        }
    }
}

