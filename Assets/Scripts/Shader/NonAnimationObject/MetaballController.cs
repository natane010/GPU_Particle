using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballController : MonoBehaviour
{
    [SerializeField] GameObject MetaballCamera;
    [SerializeField] Material MetaballCameraMaterial;
    GameObject m_mainCamera;
    Camera m_metaCamera;
    MetaBallPostProcess m_metaBallPostProcess;
    private void Start()
    {
        m_mainCamera = Camera.main.gameObject;
        Transform TransformCamera = m_mainCamera.transform;
        InstantiateCamera(TransformCamera);
        SetUpPostProcess();
    }
    void InstantiateCamera(Transform transform)
    {
        m_metaCamera = Instantiate(MetaballCamera).GetComponent<Camera>();
        m_metaCamera.transform.position = transform.position;
        m_metaCamera.transform.rotation = transform.rotation;
        m_metaCamera.transform.parent = transform;
    }
    void SetUpPostProcess()
    {
        m_metaBallPostProcess = m_mainCamera.GetComponent<MetaBallPostProcess>();
        m_metaBallPostProcess.Init(m_metaCamera, MetaballCameraMaterial);
        m_metaBallPostProcess.enabled = true;
    }
    private void OnDestroy()
    {
        m_metaBallPostProcess.enabled = false;
        if (m_mainCamera != null)
        {
            Destroy(m_metaCamera.gameObject);
        }
    }
}
