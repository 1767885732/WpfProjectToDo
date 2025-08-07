using Microsoft.EntityFrameworkCore;

namespace MyToDo.Api.Context.Repository
{

    public class ToDoRepository : Repository<ToDo>, IRepository<ToDo>
    {
        public ToDoRepository(MyToDoContext dbContext) : base(dbContext)
        {
        }
    }
}
