using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GraCLI;
using GraZaDuzoZaMalo.Model;
using static GraZaDuzoZaMalo.Model.Gra.Odpowiedz;

namespace AppGraZaDuzoZaMaloCLI
{
    public class KontrolerCLI
    {
        public const char ZNAK_ZAKONCZENIA_GRY = 'X';

        private Gra gra;
        private WidokCLI widok;
        // Można tutaj też użyć XMLGameSerializer
        private GameSaver gameSaver = new BinaryGameSerializer();

        public int MinZakres { get; private set; } = 1;
        public int MaxZakres { get; private set; } = 100;

        public IReadOnlyList<Gra.Ruch> ListaRuchow {
            get
            { return gra.ListaRuchow;  }
 }

        public KontrolerCLI()
        {
            gra = new Gra();
            widok = new WidokCLI(this);
        }

        public void Uruchom()
        {
            widok.OpisGry();
            while( widok.ChceszKontynuowac("Czy chcesz kontynuować aplikację (t/n)? ") )
                UruchomRozgrywke();
        }

        public void UruchomRozgrywke()
        {
            widok.CzyscEkran();
            // ustaw zakres do losowania

            if (gameSaver.SaveExists && widok.ChceszKontynuowac("Czy chcesz wczytać poprzednią rozgrywkę? (t/n)"))
            {
                gra = gameSaver.LoadGame();
                gra.Wznow();
                gameSaver.SerializeGame(gra);
                widok.HistoriaGry();
            } else
            {
                gra = new Gra(MinZakres, MaxZakres); //może zgłosić ArgumentException
                gameSaver.DeleteSave();
            }

            do
            {
                //wczytaj propozycję
                int propozycja = 0;
                try
                {
                    propozycja = widok.WczytajPropozycje();
                }
                catch( KoniecGryException)
                {
                    gra.Przerwij();
                    gameSaver.SerializeGame(gra);
                    Console.WriteLine("Gra została zapisana");
                    Environment.Exit(0); // wyjdź z aplikacji
                }

                Console.WriteLine(propozycja);

                if (gra.StatusGry == Gra.Status.Poddana) break;

                //Console.WriteLine( gra.Ocena(propozycja) );
                //oceń propozycję, break
                switch( gra.Ocena(propozycja) )
                {
                    case ZaDuzo:
                        widok.KomunikatZaDuzo();
                        break;
                    case ZaMalo:
                        widok.KomunikatZaMalo();
                        break;
                    case Trafiony:
                        widok.KomunikatTrafiono();
                        break;
                    default:
                        break;
                }
                widok.HistoriaGry();
            }
            while (gra.StatusGry == Gra.Status.WTrakcie);
                      
            //if StatusGry == Przerwana wypisz poprawną odpowiedź
            if (gra.StatusGry == Gra.Status.Zakonczona)
            {
                gameSaver.DeleteSave();
                widok.HistoriaGry();
            }
        }

        ///////////////////////

        public void UstawZakresDoLosowania(ref int min, ref int max)
        {

        }

        public int LiczbaProb() => gra.ListaRuchow.Count();

        public void ZakonczGre()
        {
            //np. zapisuje stan gry na dysku w celu późniejszego załadowania
            //albo dopisuje wynik do Top Score
            //sprząta pamięć
            gra = null;
            widok.CzyscEkran(); //komunikat o końcu gry
            widok = null;
            System.Environment.Exit(0);
        }

        public void ZakonczRozgrywke()
        {
            gra.Przerwij();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <exception cref="KoniecGryException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <returns></returns>
        public int WczytajLiczbeLubKoniec(string value, int defaultValue )
        {
            if( string.IsNullOrEmpty(value) )
                return defaultValue;

            value = value.TrimStart().ToUpper();
            if ( value.Length>0 && value[0].Equals(ZNAK_ZAKONCZENIA_GRY))
                throw new KoniecGryException();

            //UWAGA: ponizej może zostać zgłoszony wyjątek 
            return Int32.Parse(value);
        }
    }

    [Serializable]
    internal class KoniecGryException : Exception
    {
        public KoniecGryException()
        {
        }

        public KoniecGryException(string message) : base(message)
        {
        }

        public KoniecGryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected KoniecGryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
