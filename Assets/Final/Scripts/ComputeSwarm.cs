using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ComputeSwarm : MonoBehaviour
{
    [HideInInspector]
    public int maxInstance = 500000;
    #region Exposed Swarm Properties
    public bool usingGPU = false;
    public int instanceCount = 100000;
    [Range(0.1f,1)]
    public float Damping = 0.9f;
    public float Spread = 10;
    public float FollowForce = 0.001f;
    public float NoiseFrequency = 0.001f;
    public float NoiseForce = 1;
    public Vector3 NoiseOffset = new Vector3(0,0,0);
    public bool usingFixedUpdate = false;
    #endregion

    public Mesh instanceMesh;
    public Material instanceMaterial;
    public ComputeShader computeShader;

    #region Compute Shader Buffers
    private int cachedInstanceCount = -1;
    // Storing time inside this one
    private ComputeBuffer timeBuffer;
    // Storing music properties in this one
    private ComputeBuffer musicBuffer;
    private ComputeBuffer positionBuffer;
    // Current follow point of an agent
    private ComputeBuffer velocitiesBuffer;
    // Current following point
    private ComputeBuffer followingPoint;
    // Keeping state of the agent
    private ComputeBuffer touchedBuffer;
    // Buffer of bullets positions
    private ComputeBuffer bulletsBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    #endregion

    #region CPU Arrays
    /// <summary>
    /// Positions of the agents
    /// </summary>
    Vector3[] CPUpositions;
    /// <summary>
    /// Velocities of the agents
    /// </summary>
    Vector3[] CPUvelocities;
    /// <summary>
    /// State of each agent
    /// </summary>
    float[] CPUtouched;
    #endregion

    public Material fastMaterial;
    public bool usingFastMaterial = false;
    public bool rendering = true;

    [HideInInspector]
    public float computeTime = 0;

    void Start()
    {
        Camera.onPostRender += postRender;
        initComputeBuffers();
        initCPUArrays();
        UpdateGraphicBuffer();
    }

    void Update()
    {
        if (cachedInstanceCount != instanceCount)
        {
            //ReleaseBuffers();
            //initComputeBuffers();
            //initCPUArrays();
            UpdateGraphicBuffer();
        }

        if(!usingFixedUpdate)
        {
            float time = Time.realtimeSinceStartup;
            if (usingGPU)
                UpdateComputeBuffer();
            else
                updateBuffersOnCPU();
            computeTime = Time.realtimeSinceStartup - time;
        }
        // Render
        if(!usingFastMaterial&&rendering)
        {
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 10000), argsBuffer);
        }
        cachedInstanceCount = instanceCount;
    }
    private void postRender(Camera cam)
    {
        if(usingFastMaterial&&rendering)
        {
            fastMaterial.SetPass(0);
            fastMaterial.SetBuffer("buf_Points", positionBuffer);
            Graphics.DrawProcedural(MeshTopology.Points, instanceCount);
        }
    }
    private void FixedUpdate()
    {
        if(usingFixedUpdate)
        {
            float time = Time.realtimeSinceStartup;
            if (usingGPU)
                UpdateComputeBuffer();
            else
                updateBuffersOnCPU();
            computeTime = Time.realtimeSinceStartup - time;
        }
    }

    #region Buffers Init
    void initComputeBuffers()
    {
        System.GC.Collect();
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        positionBuffer = new ComputeBuffer(maxInstance, 12);
        timeBuffer = new ComputeBuffer(1, 4); //Contains a single element (time) which is a float
        musicBuffer = new ComputeBuffer(256, 4);

        Vector3[] positions = new Vector3[maxInstance];
        for (int i = 0; i < maxInstance; i++)
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 1000.0f);
            float height = Random.Range(-100.0f, 100.0f);
            positions[i] = new Vector3(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance);
        }
        positionBuffer.SetData(positions);

        velocitiesBuffer = new ComputeBuffer(maxInstance, 12);
        Vector3[] velocities = new Vector3[maxInstance];
        for (int i = 0; i < maxInstance; i++)
        {
            velocities[i] = new Vector3(0, -1, 0);
        }
        velocitiesBuffer.SetData(velocities);

        followingPoint = new ComputeBuffer(1, 12);
        followingPoint.SetData(new[] { transform.position });

        touchedBuffer = new ComputeBuffer(maxInstance, 4);
        float[] touched = new float[maxInstance];
        for (int i = 0; i < maxInstance; i++)
        {
            touched[i] = 0;
        }
        touchedBuffer.SetData(touched);

        bulletsBuffer = new ComputeBuffer(Bullet.maxBulletNumber, 12);
        Vector3[] bullets = new Vector3[Bullet.maxBulletNumber];
        bulletsBuffer.SetData(bullets);
    }
    void initCPUArrays()
    {
        CPUpositions = new Vector3[maxInstance];
        CPUvelocities = new Vector3[maxInstance];
        CPUtouched = new float[maxInstance];
        CPUpositions = new Vector3[maxInstance];
        for (int i = 0; i < maxInstance; i++)
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 1000.0f);
            float height = Random.Range(-100.0f, 100.0f);
            CPUpositions[i] = new Vector3(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance);
        }
    }
    #endregion

    #region Buffers Update
    void UpdateGraphicBuffer()
    {
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
        instanceMaterial.SetBuffer("touchedBuffer", touchedBuffer);
        instanceMaterial.SetBuffer("followPoint", followingPoint);
        instanceMaterial.SetBuffer("musicBuffer", musicBuffer);
        // indirect args
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
    }
    void UpdateComputeBuffer()
    {
        // Compute shader
        computeShader.SetFloat("InstanceCount", instanceCount);
        computeShader.SetFloat("Damping", Damping);
        computeShader.SetFloat("Spread", Spread);
        computeShader.SetFloat("FollowForce", FollowForce);
        computeShader.SetFloat("NoiseFrequency", NoiseFrequency);
        computeShader.SetVector("NoiseOffset", NoiseOffset);
        computeShader.SetFloat("NoiseForce", NoiseForce);

        timeBuffer.SetData(new[] { Time.time });
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "Time", timeBuffer);
        computeShader.SetBuffer(kernel, "Position", positionBuffer);
        computeShader.SetBuffer(kernel, "Touched", touchedBuffer);
        computeShader.SetBuffer(kernel, "Music", musicBuffer);

        computeShader.SetBuffer(kernel, "Velocity", velocitiesBuffer);

        followingPoint.SetData(new[] { transform.position });
        computeShader.SetBuffer(kernel, "FollowPoint", followingPoint);

        bulletsBuffer.SetData(Bullet.getPositionsArray());
        computeShader.SetBuffer(kernel, "Bullets", bulletsBuffer);

        computeShader.Dispatch(kernel, instanceCount, 1, 1);
    }
    void updateBuffersOnCPU()
    {
        Vector3[] bullets = Bullet.getPositionsArray();
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 p = RandomPoint(i);
            Vector3 pos = CPUpositions[i];
            // If touched : sensible to gravity and falling to the ground
            if (CPUtouched[i] == 1)
            {
                if (pos.y < 0.5)
                {
                    CPUvelocities[i].y = Mathf.Abs(CPUvelocities[i].y);
                    CPUvelocities[i] /= 2;
                    //pos.y = Velocity[id.x].vel.y*2;
                }
                else
                {
                    CPUvelocities[i].y -= 0.01f;
                }
                pos += CPUvelocities[i];
                CPUpositions[i] = pos;
            }
            else
            {
                CPUvelocities[i] *= Damping;
                {
                    CPUvelocities[i] += (transform.position - CPUpositions[i]) * FollowForce + snoise((CPUpositions[i] + NoiseOffset) * NoiseFrequency) * NoiseForce;
                }
                for (int j = 0; j < Bullet.bullets.Count; j++)
                {
                    if (Vector3.Distance(CPUpositions[i], bullets[j]) < 5)
                    {
                        CPUtouched[i] = 1;
                        CPUvelocities[i] = p;
                        bullets[j] = new Vector3(0, 0, 0);
                    }
                }
                // Keeping them off the ground
                CPUvelocities[i].y += (1 / pos.y) * 10;

                pos += CPUvelocities[i];
                CPUpositions[i] = pos + p * Spread;
            }
        }
        positionBuffer.SetData(CPUpositions);
        touchedBuffer.SetData(CPUtouched);
        followingPoint.SetData(new[] { transform.position });
    }
    #endregion
    
    #region Monobehaviour Events
    void OnDisable()
    {
        ReleaseBuffers();
    }
    private void OnDestroy()
    {
        ReleaseBuffers();
    }
    void ReleaseBuffers()
    {
        if (touchedBuffer != null)
            touchedBuffer.Release();
        touchedBuffer = null;

        if (timeBuffer != null)
            timeBuffer.Release();
        timeBuffer = null;

        if (musicBuffer != null)
            musicBuffer.Release();
        musicBuffer = null;

        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        if (bulletsBuffer != null)
            bulletsBuffer.Release();
        bulletsBuffer = null;

        if (followingPoint != null)
            followingPoint.Release();
        followingPoint = null;

        if (velocitiesBuffer != null)
            velocitiesBuffer.Release();
        velocitiesBuffer = null;
    }
    #endregion

    #region Utilities
    float MyRandom(float u, float v)
    {
        float f = Vector2.Dot(new Vector2(12.9898f, 78.233f), new Vector2(u, v));
        float f2 = 43758.5453f * Mathf.Sin(f);
        return f2 - Mathf.Floor(f2);
    }
    Vector3 RandomPoint(float id)
    {
        float u = MyRandom(id * 0.01334f, 0.3728f) * Mathf.PI * 2;
        float z = MyRandom(0.8372f, id * 0.01197f) * 2 - 1;
        float l = MyRandom(4.438f, id * 0.01938f - 4.378f);
        Vector2 v2 = new Vector2(Mathf.Cos(u), Mathf.Sin(u)) * Mathf.Sqrt(1 - z * z);
        return new Vector3(v2.x, v2.y, z) * Mathf.Sqrt(l);
    }

    Vector3 snoise(Vector3 v)
    {
        int radius = 1;
        Vector3 dir = new Vector3(0, 0, 0);
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                for (int z = -radius; z < radius; z++)
                {
                    dir += Perlin.Noise(v + new Vector3(x, y, z)) * (new Vector3(x, y, z));
                }
            }
        }
        return dir * 5;
    }
    #endregion
}