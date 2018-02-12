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


enum AlgoParameters
{
    WORD_ENCODING_RETRY_COUNT = (int)5,
    WORD_SEARCH_OFFSET = (int)50,
    WORD_SEARCH_LENGTH = (int)10,
    WORD_FAKE_SEARCH_UPPER_BOUND = (int)3,
    WORD_ENTROPY_POOL_SIZE = (int)50,
    WORD_ENTROPY_POOL_SENSITIVITY = (int)1, // Higher the value lower the sensistivity.
    MACSIZE = (int)4
}



// Web Chaff. 
    // Implements the entropy Pool
    class EntropyCache
    {
        ArrayList EntropyPool;
        Random wordSelector;

        public EntropyCache()
        {
            EntropyPool = new ArrayList();
            wordSelector = new Random();
        }

        int EntropyPoolAdd(string[] Message)
        {
            foreach (string word in Message)
                EntropyPool.Add(word);
            return 0;
        }

        public int EntropyCacheWarmUp(string Message)
        {
            char[] arraySplit = { ' ' };

            if (null == Message) return 0;

            if (0 == EntropyPool.Count)
            {
                EntropyPoolAdd(Message.Split(arraySplit));
            }
            else
            {
                int i = wordSelector.Next((int)AlgoParameters.WORD_ENTROPY_POOL_SENSITIVITY);
                if (i == (int)AlgoParameters.WORD_ENTROPY_POOL_SENSITIVITY - 1)
                    EntropyPoolAdd(Message.Split(arraySplit));
            }

            // Cache trimming. This is important for convergence. 
            if (this.EntropyPool.Count > (int)AlgoParameters.WORD_ENTROPY_POOL_SIZE)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Trimming Cache");
                Console.ResetColor();
                do
                {
                    this.EntropyPool.RemoveAt(0);
                } while (this.EntropyPool.Count > (int)AlgoParameters.WORD_ENTROPY_POOL_SIZE);
            }
            return 0;
        }

        public string EntropyPoolGetRandom()
        {
            if (0 != EntropyPool.Count)
                return (string)EntropyPool[wordSelector.Next(EntropyPool.Count)];
            else
                return null;
        }
    }


    class WebChaffEncoder
    {
        MSNSearchService searchService;
        SearchRequest searchRequest;
        Random resultSelector;
        Random fakeSearchInserter;
        Random randomMacGenerator;
        EntropyCache EntropyPool;
        byte[] sessionkey;

        public WebChaffEncoder()
        {
            searchService = new MSNSearchService();
            searchRequest = new SearchRequest();
            resultSelector = new Random();
            fakeSearchInserter = new Random();
            randomMacGenerator = new Random();

            EntropyPool = new EntropyCache();
            sessionkey = new byte[] { 0, 7, 9,8 };
       }

        static void printColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static String PreprocessQueryResult(String queryResult,String word)
        {
            String tempstr;
            if (queryResult[queryResult.IndexOf(word)] != ' ')
                tempstr = queryResult.Insert(queryResult.IndexOf(word), " ");
            else
                tempstr = queryResult;

            if (tempstr[tempstr.IndexOf(word) + word.Length] != ' ')
                tempstr = tempstr.Insert(tempstr.IndexOf(word) + word.Length, " ");

            return tempstr;
        }

        String GenerateMACString(string[] queryResult, String word)
        {
            Boolean encodedword = false;
            String MacString = "";
            foreach (string mesgword in queryResult)
            {
                if (mesgword.Length == 0)
                    continue;

                if (String.Equals(mesgword, ""))
                    continue;

                if (String.Equals(word, mesgword) && encodedword == false)
                {
                    //Console.WriteLine("\n Word:" + mesgword + " MacString: " + ChaffMACCodeGenerator.ComputeMAC(mesgword, sessionkey));
                    MacString = MacString + ChaffMACCodeGenerator.ComputeMAC(mesgword, sessionkey);
                    encodedword = true;
                }
                else
                {
                    string randomMac; 
                    randomMac = ChaffMACCodeGenerator.RandomMAC(randomMacGenerator);
                    //Console.WriteLine("\n Word:" + mesgword + " MacString: " + randomMac);
                    MacString = MacString + randomMac;
                }
            }
            return MacString;
        }


        // Helper functions
        private void PerformFakeWebSearch(
                int count,
                EntropyCache EntropyPool,
                Random resultSelector,
                MSNSearchService searchService,
                SearchRequest searchRequest)
        {
            SearchResponse searchResponse;
            while (0 < count--)
            {
                String fakeWord = EntropyPool.EntropyPoolGetRandom();
                if (null == fakeWord)
                {
                    printColored("Entropy Cache has no entries Waiting till warm up", ConsoleColor.Black);
                    break;
                }
                printColored("Faking Search: " + fakeWord, ConsoleColor.Blue);
                searchRequest.Requests[0].Count = (int)AlgoParameters.WORD_SEARCH_OFFSET;
                searchRequest.Requests[0].Offset = resultSelector.Next((int)AlgoParameters.WORD_SEARCH_LENGTH);
                searchResponse = searchService.Search(searchRequest);
            }
        }

        // The decoding Algorithm
        public string DecodeMessage(string EncodedIMMessage,string MacString)
        {
            string DecodedMessage="";
            char[] arraySplit = { ' ' };
            string[] IMMessageWords = EncodedIMMessage.Split(arraySplit);
            int i = 0;
            foreach (string encodedword in IMMessageWords)
            {
                if (encodedword.Length == 0)
                    continue;

                if (String.Equals(encodedword, ""))
                    continue;

               
                string MacSplit;
                MacSplit = MacString.Substring(i * (int)AlgoParameters.MACSIZE, (int)AlgoParameters.MACSIZE);
                   if (ChaffMACCodeGenerator.VerifyMAC(encodedword, MacSplit, sessionkey) == true)
                    DecodedMessage = DecodedMessage + " " + encodedword;
                i++;
            }
            return DecodedMessage;
        }


        // The encoding algorithm. 
        public string EncodeMessage(string[] IMMessage,out string MacString)
        {
            int numSourceRequest = 1;
            SourceRequest[] sourceRequest = new SourceRequest[numSourceRequest];

            string EncodedMessage = "";
            string LocalMacString = "";

            sourceRequest[0] = new SourceRequest();
            sourceRequest[0].Source = SourceType.Web;
            sourceRequest[0].Count = (int)AlgoParameters.WORD_SEARCH_LENGTH;
            sourceRequest[0].Offset = resultSelector.Next((int)AlgoParameters.WORD_SEARCH_OFFSET);

            searchRequest.Query = "Chaffer";
            searchRequest.Requests = sourceRequest;
            searchRequest.AppID = "351A70F9FBE13428EF87F3FEB65F235482371425";
            searchRequest.CultureInfo = "en-US";
            searchRequest.Requests = sourceRequest;

            // Get out every single message.
            foreach (string word in IMMessage)
            {
                SearchResponse searchResponse;
                
                if (word.Length == 0)
                    continue;

                if (String.Equals(word, ""))
                    continue;

                // Fake Encoding       
                PerformFakeWebSearch((int)fakeSearchInserter.Next((int)AlgoParameters.WORD_FAKE_SEARCH_UPPER_BOUND),
                                    EntropyPool,
                                    resultSelector,
                                    searchService,
                                    searchRequest);


                //--------------Word Encoding Begin ----------------------------------------------------------------------------//
                // Encode the real word. 
                searchRequest.Query = word;
                int retry_count = (int)AlgoParameters.WORD_ENCODING_RETRY_COUNT;
                char[] arraySplit = { ' ' };
                Boolean wordEncoded = false;
                do
                {
                    searchRequest.Requests[0].Count = (int)AlgoParameters.WORD_SEARCH_OFFSET;
                    searchRequest.Requests[0].Offset = resultSelector.Next((int)AlgoParameters.WORD_SEARCH_LENGTH);
                    searchResponse = searchService.Search(searchRequest);

                    /* Select the appropriate response out of the 10 hits */
                    foreach (SourceResponse sourceResponse in searchResponse.Responses)
                    {
                        Result[] sourceResults = sourceResponse.Results;
                        int index = 0;
                        foreach (Result sourceResult in sourceResults)
                        {
                            if (sourceResult.Description != null && sourceResult.Description.Contains(word))
                            {
                                wordEncoded = true;
                                printColored(word, ConsoleColor.Green);
                                string tempstr = PreprocessQueryResult(sourceResult.Description, word);
                                EncodedMessage = EncodedMessage + tempstr + " ";
                                LocalMacString = LocalMacString + GenerateMACString(tempstr.Split(arraySplit), word);

                                // Perform the warm up close to the hit. 
                                if (index == sourceResults.GetLength(0) - 1)
                                {
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[sourceResults.GetLength(0) - 1].Description);
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[sourceResults.GetLength(0) - 2].Description);
                                }
                                else if (index == 0)
                                {
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[0].Description);
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[1].Description);
                                }
                                else
                                {
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[index + 1].Description);
                                    EntropyPool.EntropyCacheWarmUp(sourceResults[index - 1].Description);
                                }
                                EntropyPool.EntropyCacheWarmUp(sourceResult.Description);
                                break;

                            }
                            index++;
                        }
                    }
                } while (!wordEncoded && (retry_count-- > 0));

                if (!wordEncoded)
                    printColored("Oops! Could not encode word->" + word, ConsoleColor.Red);
                //--------------Word Encoding Done ----------------------------------------------------------------------------//

                // Fake Encoding       
                PerformFakeWebSearch(fakeSearchInserter.Next((int)AlgoParameters.WORD_FAKE_SEARCH_UPPER_BOUND),
                                    EntropyPool,
                                    resultSelector,
                                    searchService,
                                    searchRequest);
            }
            MacString = String.Copy(LocalMacString);
            return EncodedMessage;
        }
    }
