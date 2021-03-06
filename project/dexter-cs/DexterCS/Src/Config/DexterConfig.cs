﻿#region Copyright notice
/**
 * Copyright (c) 2018 Samsung Electronics, Inc.,
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, this
 * list of conditions and the following disclaimer.
 * 
 * * Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DexterCS
{
    public sealed class DexterConfig
    {
        private static ILog CliLog = LogManager.GetLogger(typeof(DexterConfig));

        private List<IDexterHomeListener> dexterHomeListenerList = new List<IDexterHomeListener>(3);

        public enum Run_Mode
        {
            CLI, ECLIPSE, DAEMON, INTELLIJ, NETBEANS, SOURCE_INSIGHT
        }

        public void AddDexterHomeListener(IDexterHomeListener listener)
        {
            if (dexterHomeListenerList.Contains(listener))
            {
                dexterHomeListenerList.Add(listener);
            }
        }

        public enum LANGUAGE
        {
            JAVA, JAVASCRIPT, C, CPP, C_SHARP, UNKNOWN, ALL
        };
        public enum AnalysisType
        {
            SAVE, FILE, FOLDER, PROJECT, SNAPSHOT, UNKNOWN
        };

        public bool IsSpecifiedCheckerOptionEnabledByCli { get; set; }

        public readonly String DEXTER_HOME_KEY = "dexterHome";
        private readonly string PLUGIN_FOLDER_NAME = "plugin";

        private static HashSet<string> supportingFileExtensions = new HashSet<string>();
        public void AddSupportedFileExtensions(string[] fileExtensions)
        {
            foreach (string extension in fileExtensions)
            {
                if (supportingFileExtensions.Contains(extension.ToLowerInvariant()) == false)
                {
                    supportingFileExtensions.Add(extension.ToLowerInvariant());
                }
            }
        }

        public static readonly string RESULT_FOLDER_NAME = "result";
        public static readonly string OLD_FOLDER_NAME = "old";
        public static readonly string TEMP_FOLDER_NAME = "temp";
        public static readonly string LOG_FOLDER_NAME = "log";
        public static readonly string FILTER_FOLDER_NAME = "filter";
        public static readonly string POST_ANALYSIS_RESULT_V3 = "/api/v3/analysis/result";
        public static readonly string POST_SNAPSHOT_SOURCECODE = "/api/v1/analysis/snapshot/source";

        public Run_Mode RunMode { get; set; }

        private string HomePath { get; set; }
        private string dexterHome;
        public string DexterHome
        {
            get
            {
                return dexterHome;
            }
            set
            {
                HomePath = DexterUtil.RefinePath(value);
                dexterHome = HomePath;
            }
        }

        public static HashSet<string> SupportingFileExtensions
        {
            get
            {
                return supportingFileExtensions;
            }
            set
            {
                foreach (string extension in value)
                {
                    if (supportingFileExtensions.Contains(extension.ToLowerInvariant()) == false)
                    {
                        supportingFileExtensions.Add(extension.Replace(".", "").ToLowerInvariant());
                    }
                }
            }
        }

        public void RemoveAllSupportingFileExtensions()
        {
            supportingFileExtensions.Clear();
        }

        private static DexterConfig instance = null;
        private static readonly object padlock = new object();
        public Encoding SourceEncoding = new UTF8Encoding(true);

        public static DexterConfig Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DexterConfig();
                    }
                    return instance;
                }
            }
        }

        public static readonly int SOURCE_FILE_SIZE_LIMIT = 1024 * 2 * 1024;
        public static readonly string DEXTER_CFG_FILENAME = "dexter_cfg.json";
        public string OldFilePath
        {
            get
            {
                return this.DexterHome + "/" + DexterConfig.RESULT_FOLDER_NAME + "/" + OLD_FOLDER_NAME;
            }
        }

        public void CreateInitialFolderAndFiles()
        {
            if (string.IsNullOrEmpty(DexterHome))
            {
                return;
            }
            try
            {
                DexterUtil.CreateFolderIfDoesNotExist(DexterHome);

                string bin = DexterHome + "/bin";
                DexterUtil.CreateFolderIfDoesNotExist(bin);
                DexterUtil.CreateFolderIfDoesNotExist(bin + "/cppcheck");
                DexterUtil.CreateFolderIfDoesNotExist(bin + "/cppcheck/cfg");
                DexterUtil.CreateFolderIfDoesNotExist(bin + "/dexterCS");

                string plugin = DexterHome + "/" + PLUGIN_FOLDER_NAME;
                DexterUtil.CreateFolderIfDoesNotExist(plugin);

                string result = DexterHome + "/" + RESULT_FOLDER_NAME;
                DexterUtil.CreateFolderIfDoesNotExist(result);
                DexterUtil.CreateFolderIfDoesNotExist(result + "/" + OLD_FOLDER_NAME);

                DexterUtil.CreateFolderIfDoesNotExist(DexterHome + "/" + TEMP_FOLDER_NAME);
                DexterUtil.CreateFolderIfDoesNotExist(DexterHome + "/" + LOG_FOLDER_NAME);

                string filter = DexterHome + "/" + FILTER_FOLDER_NAME;
                DexterUtil.CreateFolderIfDoesNotExist(filter);
            }
            catch (IOException)
            {
                CliLog.Error("IOException in DexterConfig");
            }
        }

        public void ChangeDexterHome(string homePath)
        {
            HomePath = DexterUtil.RefinePath(homePath);
            dexterHome = HomePath;
        }

        public bool IsFileSupportedForAnalysis(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant().Replace(".", "");
            return SupportingFileExtensions.Contains(extension);
        }
    }
}
