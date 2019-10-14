using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class testing : MonoBehaviour
{
    [SerializeField] private bool useJobs = true;
    [SerializeField] private Transform pfObjects;
    private List<Objects> objectList;

    public class Objects
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        objectList = new List<Objects>();
        for (int i = 0; i < 1000; i++)
        {
            Transform objectTransform = Instantiate(pfObjects, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            objectList.Add(new Objects
            {
                transform = objectTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }

    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs)
        {
            //NativeArray<float3> positionArray = new NativeArray<float3>(objectList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(objectList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(objectList.Count);

            for (int i = 0; i < objectList.Count; i++)
            {
                //positionArray[i] = objectList[i].transform.position;
                moveYArray[i] = objectList[i].moveY;
                transformAccessArray.Add(objectList[i].transform);
            }

            /*
            ExampleToughParallelJob exampleToughParallelJob = new ExampleToughParallelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = exampleToughParallelJob.Schedule(objectList.Count, 100);
            jobHandle.Complete();
            */

            ExampleToughParallelJobTransform exampleToughParallelJobTransform = new ExampleToughParallelJobTransform
            {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = exampleToughParallelJobTransform.Schedule(transformAccessArray);
            jobHandle.Complete();

            for (int i = 0; i < objectList.Count; i++)
            {
                //objectList[i].transform.position = positionArray[i];
                objectList[i].moveY = moveYArray[i];
            }

            //positionArray.Dispose();
            moveYArray.Dispose();
            transformAccessArray.Dispose();
        }
        else
        {
            foreach (Objects items in objectList)
            {
                items.transform.position += new Vector3(0, items.moveY * Time.deltaTime);
                if (items.transform.position.y > 5f)
                {
                    items.moveY = +math.abs(items.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 50000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }

        }
        /*if (useJobs)
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 10; i++)
            {
                JobHandle jobHandle = ExampleThoughTaskJob();
                jobHandleList.Add(jobHandle);
                //jobHandle.Complete();
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                ExampleThoughTask();
            }
        }*/
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private void ExampleThoughTask()
    {
        //representes a though task like certain pathfinding or some complex calculation
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ExampleThoughTaskJob()
    {
        ExampleThoughJob job = new ExampleThoughJob();
        return job.Schedule();
    }
}

[BurstCompile]
public struct ExampleThoughJob : IJob
{
    //Extra fields added here

    public void Execute()
    {
        //representes a though task like certain pathfinding or some complex calculation
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

public struct ExampleToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index)
    {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

public struct ExampleToughParallelJobTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if(transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}