using System;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.model.ecs;
using Xunit;

namespace com.csutil.tests.model.esc {

    public class DoEntityTaskInBackgroundTests {

        [Fact]
        public async Task SingleTask_ExecutesImmediately() {
            // Arrange
            var service = new DoEntityTaskInBackground();
            bool taskExecuted = false;
            Action myTask = () => taskExecuted = true;

            // Act
            await service.SetTaskFor("entity1", myTask);

            // Because we return Task.CompletedTask, 
            // give the background task time to run:
            await Task.Delay(100);

            // Assert
            Assert.True(taskExecuted, "Task should have executed by now.");
        }

        [Fact]
        public async Task SecondTask_WaitsForFirstToComplete() {
            // Arrange
            var service = new DoEntityTaskInBackground();
            bool firstTaskCompleted = false;
            bool secondTaskExecuted = false;

            // The first task will simulate some work with a small delay
            Action firstTask = () => {
                Thread.Sleep(200); // simulate real work
                firstTaskCompleted = true;
            };
            Action secondTask = () => secondTaskExecuted = true;

            // Act
            // Start the first task
            await service.SetTaskFor("entity1", firstTask);
            // Immediately queue the second task
            await service.SetTaskFor("entity1", secondTask);

            // Give enough time for both tasks to complete in order
            // (first -> second). In a real scenario, you might use events.
            await Task.Delay(500);

            // Assert
            Assert.True(firstTaskCompleted, "First task should have completed.");
            Assert.True(secondTaskExecuted, "Second task should have run after the first.");
        }

        [Fact]
        public async Task DifferentEntities_RunInParallel() {
            // Arrange
            var service = new DoEntityTaskInBackground();
            bool entity1TaskExecuted = false;
            bool entity2TaskExecuted = false;

            // We'll detect concurrency by time or simple flags
            var entity1Task = new Action(() => {
                Thread.Sleep(200); // some simulated work
                entity1TaskExecuted = true;
            });

            var entity2Task = new Action(() => {
                Thread.Sleep(200); // some simulated work
                entity2TaskExecuted = true;
            });

            // Act
            // Queue tasks for different entity IDs
            await service.SetTaskFor("entity1", entity1Task);
            await service.SetTaskFor("entity2", entity2Task);

            // Give time for both to complete
            await Task.Delay(250);

            // Assert
            Assert.True(entity1TaskExecuted, "Entity1's task should have completed.");
            Assert.True(entity2TaskExecuted, "Entity2's task should have completed.");
        }

        [Fact]
        public async Task MultipleQueuedTasks_ExecuteInFIFOOrder() {
            // Arrange
            var service = new DoEntityTaskInBackground();

            var executionOrder = new System.Collections.Concurrent.ConcurrentQueue<int>();

            Action task1 = () => executionOrder.Enqueue(1);
            Action task2 = () => executionOrder.Enqueue(2);
            Action task3 = () => executionOrder.Enqueue(3);

            // Act
            await service.SetTaskFor("entity1", task1);
            await service.SetTaskFor("entity1", task2);
            await service.SetTaskFor("entity1", task3);

            // Give all tasks time to run
            await Task.Delay(500);

            // Assert
            // We expect 1, and 3 in the queue (and 2 to be skipped)
            Assert.Equal(2, executionOrder.Count);

            int[] results = executionOrder.ToArray();
            Assert.Equal(new[] { 1, 3 }, results);
        }

        [Fact]
        public async Task ExceptionInFirstTask_ShouldStillRunNextTask() {
            // Arrange
            var service = new DoEntityTaskInBackground();
            bool secondTaskExecuted = false;

            Action failingTask = () => throw new InvalidOperationException("Failing Task");
            Action nextTask = () => secondTaskExecuted = true;

            // Act
            await service.SetTaskFor("entity1", failingTask);
            await service.SetTaskFor("entity1", nextTask);

            // Give time for tasks to run
            await Task.Delay(500);

            // Assert
            Assert.True(secondTaskExecuted, "Second task should still execute even if the first fails.");
        }

    }

}