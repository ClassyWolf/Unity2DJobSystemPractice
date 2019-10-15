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
    //checkbox to easily be able to test how jobs affect the processing
    [SerializeField] private bool useJobs = true;
    //prefab object, what will be spawned
    [SerializeField] private Transform pfObjects;
    //list of objects that will be spawned
    private List<Objects> objectList;
    //number of objects spawned
    [SerializeField] private int spawns = 1000;

    public class Objects
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        //initializing the objects that will be spawned
        objectList = new List<Objects>();
        for (int i = 0; i < spawns; i++)
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
            //required parameters for parllel jobs to work, requirements vary
            //positionArray required if a normal parallel job, no transformAccessArray
            //transformAccessArray required with parallel transform jobs but no positionArray

            //NativeArray<float3> positionArray = new NativeArray<float3>(objectList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(objectList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(objectList.Count);

            for (int i = 0; i < objectList.Count; i++)
            {
                //positionArray[i] = objectList[i].transform.position;
                moveYArray[i] = objectList[i].moveY;
                transformAccessArray.Add(objectList[i].transform);
            }

            //used when working with large lists
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

            //setting up a job and sending necessary parameters
            //used when working with a large number of graphical elements
            ExampleToughParallelJobTransform exampleToughParallelJobTransform = new ExampleToughParallelJobTransform
            {
                //job parameters
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            //executing the job
            JobHandle jobHandle = exampleToughParallelJobTransform.Schedule(transformAccessArray);
            //ending the job
            jobHandle.Complete();

            //changing worked on object
            for (int i = 0; i < objectList.Count; i++)
            {
                //objectList[i].transform.position = positionArray[i];
                objectList[i].moveY = moveYArray[i];
            }

            //removing completed items
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

        //used when working with heavy calculations
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

//called to preform the job function
//burst compilation (availabe under job meny) for faster process
[BurstCompile]
public struct ExampleThoughJob : IJob
{
    //Extra fields added here

    //Execute can be called the jobs "update", but in reality it is only preformed to completition once
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

//called to preform the parallel jobs
public struct ExampleToughParallelJob : IJobParallelFor
{
    //float 3 is one of the job systems own elements that is used in place of vector3 in some instances
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    //Extra fields added here

    //Execute can be called the jobs "update", but in reality it is only preformed to completition once
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

//called to preform the parallel transform jobs
public struct ExampleToughParallelJobTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    public float deltaTime;

    //Extra fields added here

    //Execute can be called the jobs "update", but in reality it is only preformed to completition once
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