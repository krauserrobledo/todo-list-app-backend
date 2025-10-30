using Domain.Abstractions.Repositories;
using Domain.Models;

namespace Application.Services
{
    public class SubtaskService
    {
        public SubtaskService(ISubtaskRepository subtaskRepository)
        {
            _subtaskRepository = subtaskRepository;
        }

        private readonly ISubtaskRepository _subtaskRepository;

    }
}
