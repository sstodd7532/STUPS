﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/21/2014
 * Time: 10:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using Nancy;
	using Nancy.ModelBinding;
	using TMX.Interfaces.Exceptions;
	using TMX.Interfaces.Server;
	using Tmx.Interfaces.Remoting;
	using Tmx.Interfaces.Types.Remoting;
    
    /// <summary>
    /// Description of TestTasksModule.
    /// </summary>
    public class TestTasksModule : NancyModule
    {
        public TestTasksModule() : base(UrnList.TestTasks_Root)
        {
            Get[UrnList.TestTasks_CurrentClient] = parameters => {
				var taskSorter = new TaskSelector();
				ITestTask actualTask = taskSorter.GetFirstLegibleTask(parameters.id);
//if (null == actualTask)
//    Console.WriteLine("null == actualTask");
//else
//    Console.WriteLine("null != actualTask");
				return null != actualTask ? Response.AsJson(actualTask).WithStatusCode(HttpStatusCode.OK) : HttpStatusCode.NotFound;
			};
            
            Put[UrnList.TestTasks_Task] = parameters => {
                ITestTask loadedTask = this.Bind<TestTask>();
                if (null == loadedTask) throw new UpdateTaskException("Failed to update task with id = " + parameters.id);
                var storedTask = TaskPool.TasksForClients.First(task => task.Id == parameters.id && task.ClientId == loadedTask.ClientId);
                storedTask.TaskFinished = loadedTask.TaskFinished;
//Console.WriteLine("task finished? {0}", loadedTask.TaskFinished);
                storedTask.TaskStatus = loadedTask.TaskStatus;
                //
//if (null == loadedTask.TaskResult)
//    Console.WriteLine("null == loadedTask.TaskResult");
//else {
//    Console.WriteLine("null != loadedTask.TaskResult");
//    foreach (var element in loadedTask.TaskResult) {
//        Console.WriteLine(element);
//    }
//}
                // storedTask.TaskResult = loadedTask.TaskResult;
                //
                
                if (storedTask.TaskFinished) {
                    var taskSorter = new TaskSelector();
                    ITestTask nextTask = null;
                    try {
                        nextTask = taskSorter.GetNextLegibleTask(storedTask.ClientId, storedTask.Id);
                    }
                    catch (Exception eFailedToGetNextTask) {
                        throw new FailedToGetNextTaskException(eFailedToGetNextTask.Message);
                    }
                    
                    if (null == nextTask) return HttpStatusCode.OK;
                    nextTask.PreviousTaskResult = storedTask.TaskResult;
                    nextTask.PreviousTaskId = storedTask.Id;
                }
                return HttpStatusCode.OK;
            };
        	
        	Put[UrnList.TestTasks_CurrentTask + UrnList.TestTasks_CurrentClient] = parameters => {
                ITestTask loadedTask = this.Bind<TestTask>();
                if (null == loadedTask) throw new UpdateTaskException("Failed to send results to task, client id = " + parameters.id);
                
                var taskSorter = new TaskSelector();
                var actualTask = taskSorter.GetFirstLegibleTask(parameters.id) as TestTask;
                // var actualTask = taskSorter.GetFirstLegibleTask(parameters.id);
                // actualTask.TaskResult = loadedTask.TaskResult;
//if (null == loadedTask.TaskResult)
//    Console.WriteLine("null == loadedTask.TaskResult");
//else {
//    Console.WriteLine("null != loadedTask.TaskResult");
//    foreach (var element in loadedTask.TaskResult) {
//        Console.WriteLine(element);
//    }
//}
                // actualTask.TaskResult = null == actualTask.TaskResult ? new string[] {} : actualTask.TaskResult.Concat(loadedTask.TaskResult).ToArray();
                // actualTask.TaskResult = loadedTask.TaskResult;
				// actualTask.TaskResult = actualTask.TaskResult ?? new string[] {};
				// actualTask.TaskResult = actualTask.TaskResult.Concat(loadedTask.TaskResult).ToArray();
				// actualTask.TaskResult = (null == actualTask.TaskResult) ? new string[] {} : actualTask.TaskResult.Concat(loadedTask.TaskResult).ToArray();
				var currentTaskResult = actualTask.TaskResult ?? new string[] {};
				actualTask.TaskResult = currentTaskResult.Concat(loadedTask.TaskResult).ToArray();
                return HttpStatusCode.OK;        		
        	};
        }
    }
}
