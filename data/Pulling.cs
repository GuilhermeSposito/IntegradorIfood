using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoIntegradorIfood.data;

public  class Pulling
{
    public string id { get; set; }

	public Pulling()
	{
			
	}

	public static void GetPullings()
	{
		using (var dbContext = new  ApplicationDbContext())
		{
			var pullings = dbContext.pulling;

            Console.WriteLine("\n\tPullings já relizados\n");
			foreach (var pulling in pullings)
			{
				Console.WriteLine($" id: {pulling.id}");
			}
        }
	}
}
