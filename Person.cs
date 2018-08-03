using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXTestProject
{
    public class Person
    {
        // state change
        // return value
        // argument change
        // throw exception
        // call 3rd party

        // no need to solve other objects updating the state of this object 'cos we normally don't pass 'this' around
        // with method calls. And we should not pass internal data structures at all.
        // however, it is possible that other objects need to be setup with this obj as collaborator before we can call our method. this
        // requires cyclic class dependency.
        private int counter = 0;
        public string Name { get; set; } = "John";
        public int Age { get; set; }
        public List<string> MyList { get; set; } = new List<string>();
        public List<string> MyList_new { get; set; } = new List<string>();
        public List<string> MyList_add { get; set; } = new List<string>();
        public List<string> MyList_addrange { get; set; } = new List<string>();
        public List<string> MyList_remove { get; set; } = new List<string>();
        public List<string> MyList_count { get; set; } = new List<string>();
        //public IList<string> MyList { get; set; } = new List<string>();
        public IEnumerable<string> MyEnumerable_clear { get; set; } = new List<string>();
        public int[] MyArray { get; set; } = new int[10];

        public string ChangeName(int num, string s)
        {
            var k = "hello wprld";
            MyList.Add("dgtrfh");
            var i = 10;
            var result = new List<string>();
            var MCDC = new List<string>();
            var separators = "hello".ToArray();
            foreach (var exp in MCDC)
            {
                var reducedExp = exp.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                var str = string.Join("", reducedExp.Select(x => x[0]));
                Name = "Bob";
                str = str.Remove(str.Length - 1); // remove the result from the expression
                result.Add(str);
                MyList.Add("jgjg");
            }

            return string.Join(", ", result);
        }

        public List<int> ChangeState() {
            var a = 0;
            var b = 3;
            var age = "";
            b = 10;
            Age = 55;
            //Age = GetAge();
            counter++;
            --counter;
            if ((a >= 0 && b < 5) || b > 10) {
                //Age = 13;
                /*int i;
                i = 15;
                a = 12;
                MyList_new = new List<string>();
                MyList_add[0] = "";
                MyList_add.Add("hi");
                MyList_addrange.AddRange(new[] { "hi", "hello" });
                MyArray[0] = 20;
                ((List<string>)MyEnumerable_clear).Clear();
                var k = ((List<string>)MyEnumerable_clear).Count();
                var c = MyList_remove.Remove("hi");*/
                //var j = 
                MyList_add.Add("hi");
                MyList_count.Count();
                MyList_count.MyExtension();
                //((List<string>)MyList)[0] = "";
                //SetAge(14);
                //counter = 13;
                //Console.WriteLine("Hello, World!");
            }
            else if (a > 30) {
                //MyList_add.Add("tutu");
                //Console.WriteLine("Hello, World!");
            }
            else {
                //MyList_add.Add("sgdg");
                //var success = SetAge(5);
            }

            return null;
        }

        private int GetAge()
        {
            throw new NotImplementedException();
        }

        private bool SetAge(int n)
        {
            Age = n;
            return true;
        }
    }

    public interface IMyInterface
    {
        void MyMethod();
    }

    public static class Extensions
    {
        public static void MyExtension<T>(this IEnumerable<T> lst)
        {

        }
    }
}
