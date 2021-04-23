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


        public bool IsUserExists(string username)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var user = entities.Users.SingleOrDefault(s => s.Username == username);
                if (user==null)
                    return false;
                else
                    return true;
            }


            
        }
        
        public int IsUserPasswordCorrect(string username,string password)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                var user = entities.Users.SingleOrDefault(s => s.Username == username && s.Password == password);
                if (user == null)
                    return -1;
                else
                    return user.Id;
            }


            
        }

      

        public async System.Threading.Tasks.Task AddUserAsync(User user)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Users.Add(user);
                await entities.SaveChangesAsync();
            }
        }

        public async Task<List<Models.Task>> GetTaskListAsync(int id)
        {
            var list = new List<Models.Task>();
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                list = await entities.Tasks.Where(t => t.UsersId == id).ToListAsync().ConfigureAwait(false);
                return list;
            }
        }

        public async System.Threading.Tasks.Task AddTaskAsync(Models.Task task)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Tasks.Add(task);
                await entities.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task RemoveTaskAsync(Models.Task task)
        {
            using (TaskViewerEntities entities = new TaskViewerEntities())
            {
                entities.Tasks.Attach(task);
                entities.Tasks.Remove(task);
                await entities.SaveChangesAsync();
            }
        }

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

    }
}