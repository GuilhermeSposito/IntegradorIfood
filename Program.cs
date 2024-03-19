using ProjetoIntegradorIfood;
using ProjetoIntegradorIfood.ClassesAuxiliares;
using ProjetoIntegradorIfood.data;
Console.ForegroundColor = ConsoleColor.White;
//Ifood.SetTimer();


while (true)
{
    try
    {
        //Menu
        Console.WriteLine($"1 - Autorizar App\n" +
            $"2 - Mostrar Pullings");
         
        switch (Convert.ToInt32(Console.ReadLine()))
        {
            case 1:
                await Ifood.Autorizacao();
                break;
            case 2:
                Pulling.GetPullings();
                break;
            case 3:
                await Ifood.SetPedido();
                break; 
        }


    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}