using System.Collections.Generic;
using System.Threading.Tasks;
using TaskViewer.Database.Models;

namespace TaskViewer.Database.Services
{
    public interface IDatabaseService
    {
        bool IsUserExists(string username);

        System.Threading.Tasks.Task AddTaskAsync(Models.Task task);

        System.Threading.Tasks.Task AddUserAsync(User user);

        System.Threading.Tasks.Task RemoveTaskAsync(Models.Task task);

        Task<List<Models.Task>> GetTaskListAsync(int id);

        int IsUserPasswordCorrect(string username, string password);

        System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task);
    }
}