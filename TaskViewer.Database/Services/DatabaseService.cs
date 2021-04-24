using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TaskViewer.Database.Models;
using TaskViewer.Database.Services;

namespace TaskViewer.Database.Services
{
    public class DatabaseService : IDatabaseService
    {
        #region Public Methods

        /// <summary>
        /// Is user with that username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns>true or false</returns>
        public bool IsUserExists(string username)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var user = entities.Users.SingleOrDefault(s => s.Username == username);
                if (user == null)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Is password for that username correct
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns> <br>true if correct</br> <br>false if not correct</br></returns>
        public bool IsUserPasswordCorrect(string username, string password)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var user = entities.Users.SingleOrDefault(s => s.Username == username && s.Password == password);
                if (user == null)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Get user id by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns><br>User.id if user found</br> <br>-1 if user not found</br> </returns>
        public int GetUserId(string username)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var user = entities.Users.SingleOrDefault(s => s.Username == username);
                if (user == null)
                    return -1;
                else
                    return user.Id;
            }
        }

        /// <summary>
        /// Add new User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task AddUserAsync(User user)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Users.Add(user);
                await entities.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get list of all user tasks
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of tasks</returns>
        public async Task<List<Models.Task>> GetTaskListAsync(int id)
        {
            var list = new List<Models.Task>();
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                list = await entities.Tasks.Where(t => t.UsersId == id).ToListAsync().ConfigureAwait(false);
                return list;
            }
        }

        /// <summary>
        /// Add new task
        /// </summary>
        /// <param name="task"></param>
        public async System.Threading.Tasks.Task AddTaskAsync(Models.Task task)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Tasks.Add(task);
                await entities.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes task
        /// </summary>
        /// <param name="task"></param>
        ///
        public async System.Threading.Tasks.Task RemoveTaskAsync(Models.Task task)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Tasks.Attach(task);
                entities.Tasks.Remove(task);
                await entities.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update task
        /// </summary>
        /// <param name="task"></param>
        public async System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var result = entities.Tasks.SingleOrDefault(b => b.Id == task.Id);
                if (result != null)
                {
                    entities.Entry(result).CurrentValues.SetValues(task);
                    await entities.SaveChangesAsync();
                }
            }
        }

        #endregion Public Methods
    }
}