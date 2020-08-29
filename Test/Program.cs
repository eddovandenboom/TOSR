using System;
using System.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Single threaded
            Console.WriteLine("Solving single threaded...");
            var tricks1 = Solver.Api.SolveBoardPBN("N:AT5.AJT.A632.KJ7 Q763.KQ9.KQJ94.T 942.87653..98653 KJ8.42.T875.AQ42");
            Console.WriteLine(tricks1 + "\n");

            // Multi threaded
            Console.WriteLine("Solving multi threaded...");
            var tricks2 = Solver.Api.SolveAllBoards(new[] {
                "N:T984.AK96.KQJ9.4 Q652.QJT53.T3.AT AKJ73.7.752.KJ62 .842.A864.Q98753" ,
                "N:KT98.AK96.J964.4 Q652.QJT53.T3.AT AJ743.7.752.KJ62 .842.AKQ8.Q98753"});
            tricks2.Take(2).ToList().ForEach(i => Console.WriteLine(i));
        }
    }
}
