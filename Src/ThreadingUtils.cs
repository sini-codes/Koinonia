using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{

    public class ThreadingUtils
    {

        public static void Initialize()
        {
            MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        }

        public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }

        public static int MainThreadId { get; private set; }

        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadId; }
        }

        public static void DispatchOnMainThread(Action x)
        {
            if (IsMainThread)
            {
                x();
                return;
            }
            

            EditorApplication.CallbackFunction callbackFunction = null;
            callbackFunction = () =>
            {
                x();
            };
            EditorApplication.delayCall += callbackFunction;
            return;
        }

        public static void WaitOnMainThread(Action x)
        {

            if (IsMainThread)
            {
                x();
                return;
            }

            EventWaitHandle s = new ManualResetEvent(false);
            EditorApplication.CallbackFunction callbackFunction = null;
            callbackFunction = () =>
            {
                x();
                s.Set();
            };
            EditorApplication.delayCall += callbackFunction;
            s.WaitOne();
            return;
        }

        public static T GetOnMainThread<T>(Func<T> selector)
        {
            if (IsMainThread)
            {
                return selector();
            }
            EventWaitHandle s = new ManualResetEvent(false);
            T result = default(T);
            EditorApplication.CallbackFunction callbackFunction = null;
            callbackFunction = () =>
            {
                result = selector();
                //EditorApplication.delayCall -= callbackFunction;
                s.Set();
            };
            EditorApplication.delayCall += callbackFunction;
            s.WaitOne();
            return result;
        }

        public static void RunJob<T>(EditorJob<T> editorJob)
        {
            var thread = new Thread(() =>
            {
                T result;
                try
                {
                    result = editorJob.Job();

                }
                catch (Exception ex)
                {
                    DispatchOnMainThread(()=>Debug.LogError("Editor Job Failed: "+ex.Message));
                    return;
                }
                DispatchOnMainThread(()=>editorJob.Callback(result));
            });
            thread.Start();
        }

        public static void RunTask (EditorTask editorTask, object argument)
        {
            var thread = new Thread(() =>
            {
                object result = null;

                try
                {
                    result = editorTask.GenericTaskFunction(argument);

                    DispatchOnMainThread(() =>
                    {
                        if (editorTask.DoneBlock != null) editorTask.DoneBlock(result);

                        if(editorTask.NextTask != null) RunTask(editorTask.NextTask,argument);
                    });

                }
                catch (Exception ex)
                {
                    if (editorTask.CatchBlock != null)
                    {
                        editorTask.CatchBlock(ex);
                    }
                    else
                    {
                        DispatchOnMainThread(() =>
                        {
                            throw ex;
                        });
                        return;
                    }
                }
                finally
                {
                    if (editorTask.FinallyBlock != null)
                    {
                        editorTask.FinallyBlock();
                    }
                }

            });
            thread.Start();
        }

    }

    public class EditorJob<T>
    {
        public Func<T> Job { get; set; }
        public Action<T> Callback { get; set; }
    }

    public class EditorTask<TInput, TOutput> : EditorTask
    {
        public EditorTask(Func<TInput, TOutput> taskFunction)
        {
            TaskFunction = taskFunction;
        }

        public Func<TInput,TOutput> TaskFunction { get; private set; }

        public override Func<object, object> GenericTaskFunction
        {
            get
            {
                return x => TaskFunction((TInput) x);
            }
            set { }
        }


        public EditorTask<TInput, TOutput> Catch(Action<Exception> handler)
        {
            CatchBlock = handler;
            return this;
        }

        public EditorTask<TInput, TOutput> Finally(Action handler)
        {
            throw new NotImplementedException("Finally block is not yet implemented.");
        }

        public EditorTask<TInput, TOutput> Done(Action<TOutput> handler)
        {
            DoneBlock = _ =>
            {
                handler((TOutput) _);
            };
            return this;
        }

        public EditorTask<TOutput, TNextOutput> ContinueWith<TNextOutput>(Func<TOutput, TNextOutput> taskJob)
        {
            var continueWith = CreateTask<TOutput, TNextOutput>(taskJob);
            NextTask = continueWith;
            return continueWith;
        }

        public EditorTask<TOutput, TNextOutput> ContinueWith<TNextOutput>(EditorTask<TOutput, TNextOutput> task)
        {
            NextTask = task;
            return task;
        }
        public EditorTask<TOtherIn, TOtherOut> ContinueWith<TOtherIn, TOtherOut>(Func<EditorTask<TOtherIn, TOtherOut>> selector)
        {

            var surrogateTask = CreateTask<TOtherIn, TOtherOut>(_ =>
            {
                var task = selector();
                return task.TaskFunction(_);
            });
            NextTask = surrogateTask;
            return surrogateTask;
        }

      


    }

    public class EditorTask
    {
        private Action<Exception> _catchBlock;
        private Action _finallyBlock;
        private Action<object> _doneBlock;
        public virtual Func<object, object> GenericTaskFunction { get; set; }

        public virtual Action<Exception> CatchBlock
        {
            get
            {
                if(_catchBlock != null) return _catchBlock;
                if (NextTask != null) return NextTask.CatchBlock;
                return null;
            }
            set { _catchBlock = value; }
        }

        public virtual Action FinallyBlock
        {
            get
            {
                if (_finallyBlock != null) return _finallyBlock;
                if (NextTask != null) return NextTask.FinallyBlock;
                return null;
            }
            set { _finallyBlock = value; }
        }

        public virtual Action<object> DoneBlock
        {
            get { return _doneBlock; }
            set { _doneBlock = value; }
        }

        public virtual EditorTask NextTask{ get; set; }


        public static EditorTask<TCInput, TCOutput> CreateTask<TCInput, TCOutput>(Func<TCInput, TCOutput> taskFunction)
        {
            var task = new EditorTask<TCInput, TCOutput>(taskFunction);
            return task;
        }

        public static EditorTask<TIn, TOut> CreateAndRun<TIn, TOut>(Func<TIn, TOut> taskFunction)
        {
            var task = CreateTask(taskFunction);
            ThreadingUtils.DispatchOnMainThread(() =>
            {
                ThreadingUtils.RunTask(task,null);
            });
            return task;
        }

        public static EditorTask<object, object> CreateAndRun(Action taskFunction)
        {
            var task = CreateTask<object,object>(_ =>
            {
                taskFunction();
                return null;
            });

            ThreadingUtils.DispatchOnMainThread(() =>
            {
                ThreadingUtils.RunTask(task,null);
            });
            return task;
        }

        public static EditorTask<object, T> CreateAndRun<T>(Func<T> taskFunction)
        {
            var task = CreateTask<object,T>(_ =>
            {
                return taskFunction();
            });

            ThreadingUtils.DispatchOnMainThread(() =>
            {
                ThreadingUtils.RunTask(task,null);
            });

            return task;
        }

        public static EditorTask<TIn, TOut> RunTask<TIn, TOut>(EditorTask<TIn, TOut> task)
        {
            ThreadingUtils.DispatchOnMainThread(() =>
            {
                ThreadingUtils.RunTask(task,null);
            });
            return task;
        }


    }

}
