using UnityEngine;

public class EnemyModel
{
    public Mesh Mesh { get; }
    public Texture Texture { get; }
    public AnimationClip Animation { get; }

    // Constructor to initialize EnemyModel with mesh, texture, and animation.
    public EnemyModel(Mesh mesh, Texture texture, AnimationClip animation)
    {
        Mesh = mesh;
        Texture = texture;
        Animation = animation;
    }
}

public class EnemySpawner : MonoBehaviour
{
    // Shared enemy models for different types.
    private EnemyModel goblinModel;
    private EnemyModel orcModel;

    private void Awake()
    {
        // Initialize shared models for goblins and orcs.
        goblinModel = new EnemyModel(
            Resources.Load<Mesh>("GoblinMesh"),
            Resources.Load<Texture>("GoblinTexture"),
            Resources.Load<AnimationClip>("GoblinAnimation")
        );

        orcModel = new EnemyModel(
            Resources.Load<Mesh>("OrcMesh"),
            Resources.Load<Texture>("OrcTexture"),
            Resources.Load<AnimationClip>("OrcAnimation")
        );
    }

    public void SpawnGoblin(Vector3 position)
    {
        // Create a new GameObject for the goblin with the shared goblin model and unique position.
        GameObject goblin = new GameObject("Goblin");
        goblin.transform.position = position;

        // Assign mesh, texture, and animation from the goblin model.
        MeshRenderer renderer = goblin.AddComponent<MeshRenderer>();
        renderer.sharedMaterial.mainTexture = goblinModel.Texture;

        MeshFilter filter = goblin.AddComponent<MeshFilter>();
        filter.mesh = goblinModel.Mesh;

        Animation animation = goblin.AddComponent<Animation>();
        animation.clip = goblinModel.Animation;
    }

    public void SpawnOrc(Vector3 position)
    {
        // Create a new GameObject for the orc with the shared orc model and unique position.
        GameObject orc = new GameObject("Orc");
        orc.transform.position = position;

        // Assign mesh, texture, and animation from the orc model.
        MeshRenderer renderer = orc.AddComponent<MeshRenderer>();
        renderer.sharedMaterial.mainTexture = orcModel.Texture;

        MeshFilter filter = orc.AddComponent<MeshFilter>();
        filter.mesh = orcModel.Mesh;

        Animation animation = orc.AddComponent<Animation>();
        animation.clip = orcModel.Animation;
    }
}
