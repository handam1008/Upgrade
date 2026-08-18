using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Test
{
    public class ThreadTest : MonoBehaviour
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        private void Start()
        {
            WorkSequence();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async void WorkSequence()
        {
            Debug.Log($"Sequence Start - {Thread.CurrentThread.ManagedThreadId}");
            
            int result = await WorkAsync();
            Debug.Log(result);
        }

        private async Task<int> WorkAsync()
        {
            await Task.Delay(1000);
            return 1;
        }

        private void WorkJob()
        {
            ulong i = 1;
            while (i < 4L)
            {
                Debug.Log($"Hello thread {i} - {Thread.CurrentThread.ManagedThreadId}");
                i++;
                Thread.Sleep(1000); //1초 휴식
                
                if(_cts.Token.IsCancellationRequested)
                    break;
            }
        }
    }
}