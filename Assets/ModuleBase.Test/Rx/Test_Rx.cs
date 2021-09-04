using ModuleBased.Rx;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test.Rx
{
    public class Test_Rx
    {
        /*
         * Test runner would not run coroutine.
         * So the observable handled by coroutine would not succeed.
         */
        //[Test]
        //public void Test_FromCoroutine()
        //{
        //    Debug.Log("<--- Test_FromCoroutine -->");
        //    Debug.Log("= NormalEnum without token =");
        //    Observable.FromCoroutine(RxTool.GetWrappedEnum(RxTool.NormalEum, "Finish"))
        //        .Subscribe(
        //        (result) => Assert.AreEqual("Finish", result, "Finish"),
        //        (error) => Assert.Null(error),
        //        () => Assert.Pass());

        //    Debug.Log("= ErrorEnum without token =");
        //    Observable.FromCoroutine(RxTool.GetWrappedEnum(RxTool.ErrorEnum, "Finish"))
        //        .Subscribe(
        //        (result) => Assert.Null(result),
        //        (error) => { Assert.NotNull(error); },
        //        () => Assert.Pass());

        //    Debug.Log("= NormalEnum with token =");
        //    IDisposable disposable = Observable.FromCoroutine(RxTool.GetWrappedEnum(RxTool.NormalEumWithToken, "Finish"))
        //        .Subscribe(
        //        (result) => Assert.AreEqual("Finish", result, "Finish"),
        //        (error) => { Assert.Fail(); },
        //        () => Assert.Pass());
        //    disposable.Dispose();

        //    Debug.Log("= ErrorEnum with token =");
        //    disposable = Observable.FromCoroutine(RxTool.GetWrappedEnum(RxTool.ErrorEnumWithToken, "Finish"))
        //        .Subscribe(
        //        (result) => { Assert.Null(result); Assert.Fail(); },
        //        (error) => { Assert.NotNull(error); Assert.Pass(); },
        //        () => Assert.Pass());
        //}

        //[Test]
        //public void Test_Task()
        //{
        //    Observable.Task((token) => RxTool.NormalTask(token, "Finish"))
        //        .Subscribe(
        //        (result) => { Assert.AreEqual("Finish", result); },
        //        (error) => { Assert.Fail(); },
        //        () => { Debug.Log("Complete1"); });


        //    Observable.Task((token) => RxTool.ErrorTask(token, "Finish"))
        //        .Subscribe(
        //        (result) => { Assert.Fail(); },
        //        (error) => { Assert.IsNotNull(error); },
        //        () => { Assert.Pass(); });

        //    IDisposable disposable = Observable.Task((token) => RxTool.NormalTask(token, "Finish"))
        //        .Subscribe(
        //        (result) => { Assert.AreEqual("Finish", result); },
        //        (error) => { Assert.Fail(); },
        //        () => { Assert.Pass(); });
        //    disposable.Dispose();

        //    disposable = Observable.Task((token) => RxTool.ErrorTask(token, "Finish"))
        //        .Subscribe(
        //        (result) => { Assert.Fail(); },
        //        (error) => { Assert.IsNotNull(error); },
        //        () => { Assert.Pass(); });
        //    disposable.Dispose();
        //}

        [Test]
        public void Test_Select()
        {
            int[] array = new int[] { 0, 1, 2, 3, 4 };

            Observable.FromSelect(array, (i) => i + 1)
                .Subscribe(
                (result) =>
                {
                    int index = 0;
                    foreach (var r in result)
                    {
                        Assert.AreEqual(index + 1, r);
                        index++;
                    }
                },
                (error) => { Assert.IsNull(error); },
                () => { Assert.Pass(); });

            Observable.FromSelect<int, int>(array, (i) => throw RxTool.TestError)
                .Subscribe(
                (result) => { Assert.Fail(); },
                (error) => { Assert.IsNotNull(error); },
                () => { Assert.Pass(); });
        }

        [Test]
        public void Test_ForEach()
        {
            TestObserver observer = new TestObserver();
            int[] array = new int[] { 0, 1, 2, 3, 4 };
            Observable.ForEach(array, (i) =>
            {
                return Observable.Task((token) => RxTool.NormalTask(token, i + 1));
            }).WhenAll()
            .Subscribe(
                //observer,
                (result) => {
                    Debug.Log("Got result");
                    int index = 0;
                    foreach (var r in result)
                    {
                        Assert.AreEqual(index + 1, r);
                        index++;
                    }
                },
                (error) => { Assert.Fail(); },
                () => { Debug.Log("Completed"); });

            //observer.Wait(5000, Assert.Pass, () => Assert.Fail("timeout"));
        }
    }
}