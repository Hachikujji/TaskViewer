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
        private readonly TaskViewerEntities _entities;

        public DatabaseService(TaskViewerEntities entities)
        {
            _entities = entities;
        }

        #region Public Methods

        /// <summary>
        /// Is user with that username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns>true or false</returns>
        public bool IsUserExists(string username)
        {
            var user = _entities.Users.SingleOrDefault(s => s.Username == username);
            return user != null;
        }

        /// <summary>
        /// Is password for that username correct
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns> <br>true if correct</br> <br>false if not correct</br></returns>
        public bool IsUserPasswordCorrect(string username, string password)
        {
            var user = _entities.Users.SingleOrDefault(s => s.Username == username && s.Password == password);
            return user != null;
        }

        /// <summary>
        /// Get user id by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns><br>User.id if user found</br> <br>-1 if user not found</br> </returns>
        public int GetUserId(string username)
        {
            var user = _entities.Users.SingleOrDefault(s => s.Username == username);
            if (user == null)
                return -1;
            else
                return user.Id;
        }

        /// <summary>
        /// Add new User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task AddUserAsync(User user)
        {
            _entities.Users.Add(user);
            await _entities.SaveChangesAsync();
        }

        /// <summary>
        /// Get list of all user tasks
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of tasks</returns>
        public async Task<List<Models.Task>> GetTaskListAsync(int id)
        {
            var list = new List<Models.Task>();
            list = await _entities.Tasks.Where(t => t.UserId == id).ToListAsync().ConfigureAwait(false);
            return list;
        }

        /// <summary>
        /// Add new task
        /// </summary>
        /// <param name="task"></param>
        public async System.Threading.Tasks.Task AddTaskAsync(Models.Task task)
        {
            _entities.Tasks.Add(task);
            await _entities.SaveChangesAsync();
        }

        /// <summary>
        /// Removes task
        /// </summary>
        /// <param name="task"></param>
        ///
        public async System.Threading.Tasks.Task RemoveTaskAsync(Models.Task task)
        {
            _entities.Tasks.Attach(task);
            _entities.Tasks.Remove(task);
            await _entities.SaveChangesAsync();
        }

        /// <summary>
        /// Update task
        /// </summary>
        /// <param name="task"></param>
        public async System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task)
        {
            var result = _entities.Tasks.SingleOrDefault(b => b.Id == task.Id);
            if (result != null)
            {
                _entities.Entry(result).CurrentValues.SetValues(task);
                await _entities.SaveChangesAsync();
            }
        }

        #endregion Public Methods
    }
}