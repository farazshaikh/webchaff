/* 
 * Author: Faraz Shaikh
 * Protype: Implementation for generation of chaff using Search results.
 *          Chaffs the search stream 
 *          Maintains a entroy pool of words to chaff the search stream.
 *          
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Services.Protocols;
using System.Net;
using ConsoleSampleWebSearch.LiveSearch;
using System.Collections;



// Web Chaff. 
namespace ConsoleSampleWebSearch
{
    class Program
    {
   
       static void Main(string[] args)
        {
            WebChaffEncoder Encoder = new WebChaffEncoder();
            while (true)
            {
          
                
                String userText;
                string MacString="";
                string EncodedMessage;
                char[]   arraySplit = { ' ' };
                Console.WriteLine("\nFaraz:");

                
                
                userText = Console.ReadLine();

                System.DateTime startTime = System.DateTime.Now;
                EncodedMessage = Encoder.EncodeMessage(userText.Split(arraySplit),out MacString);
                Console.WriteLine("\nEncodedTo:" + EncodedMessage);
                Console.WriteLine("\n MacString is:" + MacString);
                System.DateTime stopTime = System.DateTime.Now;
                Console.WriteLine("\nDecoded Message:" + Encoder.DecodeMessage(EncodedMessage, MacString));

                TimeSpan duration = stopTime - startTime;
                Console.WriteLine("Encoding time:" + duration);
            }
        }
    }
}