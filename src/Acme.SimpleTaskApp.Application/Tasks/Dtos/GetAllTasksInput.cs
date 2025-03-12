using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Tasks.Dtos
{
    public class GetAllTasksInput
    {
        public TaskState? State { get; set; }
    }
}
