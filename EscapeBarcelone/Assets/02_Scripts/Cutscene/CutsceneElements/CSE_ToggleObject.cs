using UnityEngine;

public class CSE_ToggleObject : CutsceneElementBase
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool enable;
    [SerializeField] private bool updateTransform;

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;

    public override void Execute()
    {
        base.Execute();

        if (target)
        {
            target.SetActive(enable);

            if (updateTransform)
            {
                target.transform.position = position;
                target.transform.rotation = Quaternion.Euler(rotation);
            }
        }

        cutsceneHandler.PlayNextElement();
    }
}