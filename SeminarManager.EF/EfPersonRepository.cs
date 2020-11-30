using System.Collections.Generic;
using SeminarManager.Model;
using System.Linq;
using SimpleHashing.Net;

namespace SeminarManager.EF
{
    public class EfPersonRepository : IPersonRepository
    {
        private ISimpleHash simpleHash = new SimpleHash();
        private SeminarManagerContext context;

        public EfPersonRepository(SeminarManagerContext context)
        {
            this.context = context;
        }

        public List<Person> All(int from = 0, int max = 1000)
        {
            return context.Persons.Skip(from).Take(max).ToList();
        }

        public Person ById(int id)
        {
            return (from obj in context.Persons where obj.ID == id select obj).FirstOrDefault();
        }

        public void Delete(int id)
        {
            var obj = new Person() { ID =id};
            context.Persons.Attach(obj);
            context.Entry(obj).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            context.SaveChanges();
        }

        public Person FindAdminByEmailAndPassword(LoginModel login)
        {
            var list = (from obj in context.Persons where obj.EMail.Equals(login.Email) select obj).ToList();
            foreach (var obj in list)
                if (simpleHash.Verify(login.Password, obj.Password))
                    return obj;
            
            return null;
        }

        public void Save(Person obj)
        {
            obj.Password = obj.IsAdmin ? simpleHash.Compute(obj.Password) : string.Empty;
            context.Add(obj);
            context.SaveChanges();
        }
    }
}