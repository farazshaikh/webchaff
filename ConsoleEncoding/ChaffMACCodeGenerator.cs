/* 
 * Author: Faraz Shaikh
 * Generates Message Authentication Code for Messages words       
 */

using System;
using System.Text;
using System.Security.Cryptography;


public class ChaffMACCodeGenerator
{
    public static string ComputeMAC(string   IMMessage,
                                     byte[]   SessionKey)
    {
        if (SessionKey == null)
        {
            Console.WriteLine("session key not set");
            return null;
        }
        
        
        byte[] IMMessagetBytes = Encoding.UTF8.GetBytes(IMMessage);
        
         byte[] IMMessageWithKeyBytes = 
                new byte[IMMessagetBytes.Length + SessionKey.Length];

        
        for (int i=0; i < IMMessagetBytes.Length; i++)
            IMMessageWithKeyBytes[i] = IMMessagetBytes[i];
        
        
        for (int i=0; i < SessionKey.Length; i++)
            IMMessageWithKeyBytes[IMMessagetBytes.Length + i] = SessionKey[i];

        
        HashAlgorithm hashlib;
        hashlib = new SHA1Managed();

        

        byte[] hashBytes = hashlib.ComputeHash(IMMessageWithKeyBytes);
        byte[] hashWithKey = new byte[hashBytes.Length + 
                                            SessionKey.Length];
        for (int i=0; i < hashBytes.Length; i++)
            hashWithKey[i] = hashBytes[i];
            

        for (int i=0; i < SessionKey.Length; i++)
            hashWithKey[hashBytes.Length + i] = SessionKey[i];
            

        string messageCode = Convert.ToBase64String(hashWithKey);
        messageCode = messageCode.Remove((int)AlgoParameters.MACSIZE, messageCode.Length - (int)AlgoParameters.MACSIZE);
        return messageCode;
    }

    
    public static bool VerifyMAC(string   IMMessage,
                                  string   RecievedMessageCode,
                                  byte[]   SessionKey)
    {
        byte[] hashWithSaltBytes = Convert.FromBase64String(RecievedMessageCode);
           string expectedHashString =
                    ComputeMAC(IMMessage, SessionKey);

        // If the computed hash matches the specified hash,
        // the plain text value must be correct.
        return (RecievedMessageCode == expectedHashString);
    }

    public static string RandomMAC(Random sessionRandom)
    {
        byte[] hashWithKey = new byte[10];
       
        
        sessionRandom.NextBytes(hashWithKey); ;


        

        string messageCode = Convert.ToBase64String(hashWithKey);
        messageCode = messageCode.Remove((int)AlgoParameters.MACSIZE, messageCode.Length - (int)AlgoParameters.MACSIZE);
        return messageCode;
    }
}
