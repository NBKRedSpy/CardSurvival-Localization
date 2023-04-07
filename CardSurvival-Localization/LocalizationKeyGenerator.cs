﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    public class LocalizationKeyGenerator
    {

        private static SHA1 Sha1 = SHA1.Create();

        /// <summary>
        /// Returns a hash based key, in the exact same format as LocalizationStringUtility.cs,
        /// Located at https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility.
        /// </summary>
        /// <remarks>This utility uses the same key code as LocalizationStringUtility, which uses
        /// a hash to create the LocalizationKey.
        /// 
        /// In a perfect world, all the cards would have a LocalizationKey and that key would be unique for every
        /// card that is generated by code.
        /// 
        /// Using a hash is a hack to simplify DLL's that use DefaultText to search for an action from 
        /// different mods.
        /// 
        /// By using a hash for the key, it is easy to search for the key in other mods and still be able 
        /// to change the DefaultText to English.  The referring mods still need to be changed to use
        /// LocalizationKey, but this just makes it easier to sync up the key since it is a constant value.
        /// </remarks>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Create(string text)
        {
            //----Set LocalizationKey
            const string prefix = "T-";
            string key = prefix + Convert.ToBase64String(Sha1.ComputeHash(UTF8Encoding.UTF8.GetBytes(text)));

            return key;
        }

    }
}
