﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.Tasks;
using Xunit;
using Xunit.Abstractions;
using SystemTasks = System.Threading.Tasks;

namespace Microsoft.Coyote.SystematicTesting.Tests.Tasks
{
    public class TaskCompletionSourceTests : BaseSystematicTest
    {
        public TaskCompletionSourceTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact(Timeout = 5000)]
        public void TestSetResult()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetResult(3);
                int result = await tcs.Task;
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.RanToCompletion,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result is 3, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestTrySetResult()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetResult(3);
                bool check = tcs.TrySetResult(5);
                int result = await tcs.Task;
                Specification.Assert(!check, "Cannot set result again.");
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.RanToCompletion,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result is 3, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestAsynchronousSetResult()
        {
            this.Test(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                var task = Task.Run(async () =>
                {
                    return await tcs.Task;
                });

                tcs.SetResult(3);
                int result = await task;
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.RanToCompletion,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result is 3, "Found unexpected value {0}.", result);
            },
            configuration: GetConfiguration().WithTestingIterations(200));
        }

        [Fact(Timeout = 5000)]
        public void TestAsynchronousSetResultTask()
        {
            this.Test(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                var task1 = Task.Run(async () =>
                {
                    return await tcs.Task;
                });

                var task2 = Task.Run(() =>
                {
                    tcs.SetResult(3);
                });

                int result = await task1;
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.RanToCompletion,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result is 3, "Found unexpected value {0}.", result);
            },
            configuration: GetConfiguration().WithTestingIterations(200));
        }

        [Fact(Timeout = 5000)]
        public void TestAsynchronousSetResultWithTwoAwaiters()
        {
            this.Test(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                var task1 = Task.Run(async () =>
                {
                    return await tcs.Task;
                });

                var task2 = Task.Run(async () =>
                {
                    return await tcs.Task;
                });

                tcs.SetResult(3);
                await Task.WhenAll(task1, task2);
                int result1 = task1.Result;
                int result2 = task2.Result;
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.RanToCompletion,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result1 is 3, "Found unexpected value {0}.", result1);
                Specification.Assert(result2 is 3, "Found unexpected value {0}.", result2);
            },
            configuration: GetConfiguration().WithTestingIterations(200));
        }

        [Fact(Timeout = 5000)]
        public void TestSetCanceled()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetCanceled();

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is OperationCanceledException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Canceled,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestTrySetCanceled()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetCanceled();
                bool check = tcs.TrySetCanceled();

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(!check, "Cannot set result again.");
                Specification.Assert(exception is OperationCanceledException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Canceled,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestAsynchronousSetCanceled()
        {
            this.Test(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                var task = Task.Run(() =>
                {
                    tcs.SetCanceled();
                });

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is OperationCanceledException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Canceled,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
            },
            configuration: GetConfiguration().WithTestingIterations(200));
        }

        [Fact(Timeout = 5000)]
        public void TestSetException()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetException(new InvalidOperationException());

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Faulted,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestTrySetException()
        {
            this.TestWithError(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetException(new InvalidOperationException());
                bool check = tcs.TrySetException(new NotImplementedException());

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(!check, "Cannot set result again.");
                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Faulted,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestAsynchronousSetException()
        {
            this.Test(async () =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                var task = Task.Run(() =>
                {
                    tcs.SetException(new InvalidOperationException());
                });

                int result = default;
                Exception exception = null;
                try
                {
                    result = await tcs.Task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(tcs.Task.Status is SystemTasks.TaskStatus.Faulted,
                    "Found unexpected status {0}.", tcs.Task.Status);
                Specification.Assert(result == default, "Found unexpected value {0}.", result);
            },
            configuration: GetConfiguration().WithTestingIterations(200));
        }

        [Fact(Timeout = 5000)]
        public void TestInvalidSetResult()
        {
            this.TestWithError(() =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetResult(3);

                Exception exception = null;
                try
                {
                    tcs.SetResult(3);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestInvalidSetCanceled()
        {
            this.TestWithError(() =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetResult(3);

                Exception exception = null;
                try
                {
                    tcs.SetCanceled();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }

        [Fact(Timeout = 5000)]
        public void TestInvalidSetException()
        {
            this.TestWithError(() =>
            {
                var tcs = TaskCompletionSource.Create<int>();
                tcs.SetResult(3);

                Exception exception = null;
                try
                {
                    tcs.SetException(new InvalidOperationException());
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Specification.Assert(exception is InvalidOperationException,
                    "Threw unexpected exception {0}.", exception.GetType());
                Specification.Assert(false, "Reached test assertion.");
            },
            expectedError: "Reached test assertion.",
            replay: true);
        }
    }
}
