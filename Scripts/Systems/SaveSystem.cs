using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.IO.Compression;

namespace Kutil {
    public static class SaveSystem {
        [System.Serializable]
        public class SaveBuilder {
            public enum SerializeType {
                NONE, // = plaintext
                JSON, JSONPRETTY,
                XML,
                // TODO
                YAML,
                // CUSTOM,
                // others?
            }
            [System.Serializable]
            public class SaveBuilderData {
                public bool isSave = true;
                // file
                public string filepath;
                public string filename;
                public bool hasCustomExtension = false;
                public string fileExtension;
                public bool shouldCreateDirIfDoesntExistOnSave = false;
                public bool shouldSaveOverwrite = false;
                public bool shouldSaveIncrement = false;
                // data
                public SerializeType serializeType;
                public object content;
                public string contentStr;
                public byte[] contentBytes;
                public bool saveAsBytes = false;
                // extra
                public Encoding encoding = Encoding.Default;
                public bool quiet = false;
                // todo test encryption save and load and xml
                public bool useEncryption = false;
                public byte[] encryptionKey;
                public bool isCompressedZip = false;
                public System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            }
            // todo web option? 
            // todo? other preset save locations? remote?
            // todo? playerprefs? 
            // todo? utility functions like get compressed data, but dont save?
            // todo? steam integration? epic games online service integration?
            public SaveBuilderData data = new SaveBuilderData();

            private string fullPath => data.filepath + data.filename + data.fileExtension;

            public SaveBuilder() { }
            public SaveBuilder(bool isSave = true) { data.isSave = isSave; }

            private bool IsReady() {
                return (data.filepath != null && data.filename != null && data.fileExtension != null
                    && data.filepath != "");
                // everything else can be null or its default
            }
            private static bool TryConvertFromJSON<T>(string s, out T tContent) {
                try {
                    tContent = JsonUtility.FromJson<T>(s);
                    if (tContent == null) {
                        Debug.LogError($"Load failed: failed to convert '{s}' JSON to {typeof(T).Name}");
                        return false;
                    }
                    return true;
                } catch (System.Exception e) {
                    Debug.LogError($"Load failed: failed to convert '{s}' JSON to {typeof(T).Name}. \nError: {e.ToString()}");
                    tContent = default;
                    return false;
                }
            }
            private static bool TryConvertFromXML<T>(string s, out T tContent) {
                try {
                    XmlSerializer xmlser = new XmlSerializer(typeof(T));
                    using (var stringReader = new StringReader(s)) {
                        using (XmlReader reader = XmlReader.Create(stringReader)) {
                            if (xmlser.CanDeserialize(reader)) {
                                tContent = (T)xmlser.Deserialize(reader);
                                return true;
                            }
                            Debug.LogError($"Load failed: cannot to convert '{s}' XML to {typeof(T).Name}");
                            tContent = default;
                            return false;
                        }
                    }
                } catch (System.Exception e) {
                    Debug.LogError($"Load failed: failed to convert '{s}' XML to {typeof(T).Name}. \nError: {e.ToString()}");
                    tContent = default;
                    return false;
                }
            }
            private static bool ConvertToXML(object obj, out string outstring) {
                XmlSerializer xmlser = new XmlSerializer(obj.GetType());
                using (var stringWriter = new StringWriter()) {
                    using (XmlWriter writer = XmlWriter.Create(stringWriter)) {
                        xmlser.Serialize(writer, obj);
                        outstring = stringWriter.ToString();
                        // always successful?
                        return true;
                    }
                }
            }

            /// <summary>
            /// Actually save content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool Save() {
                if (!IsReady()) {
                    Debug.LogError("Save failed: Some SaveBuilder data not set");
                    return false;
                }
                // serialize content
                if (data.serializeType == SerializeType.JSON || data.serializeType == SerializeType.JSONPRETTY) {
                    if (data.content == null) {
                        Debug.LogError("Save failed: cant serialize, object content not set!");
                        return false;
                    }
                    // json
                    data.contentStr = JsonUtility.ToJson(data.content, data.serializeType == SerializeType.JSONPRETTY);
                } else if (data.serializeType == SerializeType.NONE) {// = plaintext
                    if (data.contentStr == null) {
                        Debug.LogError("Save failed: string content not set!");
                        return false;
                    }
                } else if (data.serializeType == SerializeType.XML) {
                    if (data.content == null) {
                        Debug.LogError("Save failed: cant serialize, object content not set!");
                        return false;
                    }
                    ConvertToXML(data.content, out data.contentStr);
                }
                // check data
                if (data.saveAsBytes) {
                    if (data.contentBytes == null) {
                        if (data.contentStr != null) {
                            data.contentBytes = data.encoding.GetBytes(data.contentStr);
                        }
                    }
                    if (data.contentBytes == null) {
                        if (data.contentStr != null) {
                            Debug.LogError($"Binary Save failed: unsupported serialization type {data.serializeType.ToString()}");
                        } else {
                            Debug.LogError($"Binary Save failed: bytes not set");
                        }
                        return false;
                    }
                } else {
                    if (data.contentStr == null) {
                        Debug.LogError($"Plaintext Save failed: no content or unsupported serialization type {data.serializeType.ToString()}");
                        return false;
                    }
                }
                // encryption
                if (data.useEncryption) {
                    if (data.encryptionKey == null) {
                        Debug.LogError($"Save failed: using encryption but no key set");
                        return false;
                    }
                    if (data.saveAsBytes) {
                        data.contentBytes = EncryptBytes(data.contentBytes, data.encryptionKey);
                    } else {
                        data.contentStr = EncryptString(data.contentStr, data.encryptionKey);
                    }
                }
                // compression
                if (data.isCompressedZip) {
                    if (data.saveAsBytes) {
                        data.contentBytes = CompressBytes(data.contentBytes, data.compressionLevel);
                    } else {
                        Debug.LogWarning("Cannot compress and save as plaintext!");
                        // data.contentStr = EncryptString(data.contentStr, data.encryptionKey);
                    }
                }
                // check directory
                if (!Directory.Exists(data.filepath)) {
                    if (data.shouldCreateDirIfDoesntExistOnSave) {
                        Directory.CreateDirectory(data.filepath);
                    } else {
                        Debug.LogError($"Save failed: directory '{data.filepath}' does not exist");
                        return false;
                    }
                }
                // check overwrite
                string savepath = data.filepath + data.filename;
                string incrementer = "";
                bool baseFileExists = File.Exists(fullPath);
                if (baseFileExists && !data.shouldSaveOverwrite && !data.shouldSaveIncrement) {
                    Debug.LogError($"Save failed: File '{fullPath}' already exists");
                    return false;
                }
                // increment filename
                if (baseFileExists && data.shouldSaveIncrement && !data.shouldSaveOverwrite) {
                    incrementer = GetIncrementInfix(savepath, data.fileExtension);
                }
                // save
                string saveFullPath = savepath + incrementer + data.fileExtension;
                if (data.saveAsBytes) {
                    File.WriteAllBytes(saveFullPath, data.contentBytes);
                } else {
                    File.WriteAllText(saveFullPath, data.contentStr, data.encoding);
                }
                if (!data.quiet) Debug.Log($"Saved to '{saveFullPath}'");
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                return true;
            }

            private string GetIncrementInfix(string savepath, string fileExtension) {
                string incrementer;
                int saveNum = 1;
                incrementer = "_" + saveNum;
                // savepath = RemoveIncrementerInfix(savepath); //?
                while (File.Exists(savepath + incrementer + fileExtension)) {
                    saveNum++;
                    incrementer = "_" + saveNum;
                }
                return incrementer;
            }
            private static string RemoveIncrementerInfix(string fullpath) {
                int from = fullpath.LastIndexOf('_');
                int to = fullpath.LastIndexOf('.') - 1;
                if (from >= 0 && to > 0) {
                    return fullpath.Remove(from, to - from);
                } else {
                    // no incrementor found
                    return fullpath;
                }
            }

            public bool TryLoadText(out string text) {
                if (!IsReady()) {
                    Debug.LogError("Load Failed: Some SaveBuilder data not set!");
                    text = default;
                    return false;
                }
                if (File.Exists(fullPath)) {
                    text = File.ReadAllText(fullPath);
                    // encryption
                    if (data.useEncryption) {
                        if (data.encryptionKey == null) {
                            Debug.LogError($"Load failed: using encryption but no key set");
                            return false;
                        }
                        text = DecryptString(text, data.encryptionKey);
                    }
                    if (!data.quiet) Debug.Log($"Loaded '{fullPath}'");
                    return true;
                }
                Debug.LogError($"Load Failed: File '{fullPath}' does not exist");
                text = null;
                return false;
            }
            public bool TryLoadBytes(out byte[] bytes) {
                if (!IsReady()) {
                    Debug.LogError("Load Failed: Some SaveBuilder data not set!");
                    bytes = default;
                    return false;
                }
                if (File.Exists(fullPath)) {
                    bytes = File.ReadAllBytes(fullPath);
                    // decompression
                    if (data.isCompressedZip) {
                        bytes = DecompressBytes(bytes);
                    }
                    // encryption
                    if (data.useEncryption) {
                        if (data.encryptionKey == null) {
                            Debug.LogError($"Load failed: using encryption but no key set");
                            return false;
                        }
                        bytes = DecryptBytes(bytes, data.encryptionKey);
                    }
                    if (!data.quiet) Debug.Log($"Loaded '{fullPath}'");
                    return true;
                }
                Debug.LogError($"Load Failed: File '{fullPath}' does not exist");
                bytes = default;
                return false;
            }
            /// <summary>
            /// Actually load content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool TryLoad<T>(out T loadContent) {
                if (!IsReady()) {
                    Debug.LogError("Some SaveBuilder data not set!");
                    loadContent = default;
                    return false;
                }
                if (data.saveAsBytes) {
                    if (TryLoadBytes(out var b)) {
                        char[] chars = data.encoding.GetChars(b);
                        data.contentStr = new string(chars);
                    }
                } else {
                    TryLoadText(out data.contentStr);
                }
                if (data.contentStr != null) {
                    if (data.serializeType == SerializeType.JSON || data.serializeType == SerializeType.JSONPRETTY) {
                        if (TryConvertFromJSON<T>(data.contentStr, out loadContent)) {
                            // loaded and converted successfully
                            return true;
                        }
                    } else if (data.serializeType == SerializeType.XML) {
                        if (TryConvertFromXML<T>(data.contentStr, out loadContent)) {
                            return true;
                        }
                    } else {
                        Debug.LogError($"Load failed: unsupported serialization type {data.serializeType.ToString()}");
                        loadContent = default;
                        return false;
                    }
                    // otherwise cannot convert text to object type 
                }
                Debug.LogError($"Load failed: no data");
                loadContent = default;
                return false;
            }


            public SaveBuilder Clear() {
                this.data = new SaveBuilderData();
                return this;
            }
            public SaveBuilder Content(object saveObject) {
                this.data.content = saveObject;
                return this;
            }
            public SaveBuilder Content(byte[] contentBytes) {
                this.data.contentBytes = contentBytes;
                return this.AsBinary();
            }
            public SaveBuilder Content(string contentStr) {
                this.data.contentStr = contentStr;
                if (this.data.serializeType == SerializeType.NONE) {
                    return this.AsText();
                }
                return this;
            }
            public SaveBuilder Content<T>(T saveObject) {
                this.data.content = (object)saveObject;// todo test
                return this;
            }
            public SaveBuilder InLocalPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = localPath;
                return this;
            }
            public SaveBuilder InLocalDataPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = localDataPath;
                return this;
            }
            public SaveBuilder InPersistentDataPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = persistentPath;
                return this;
            }
            public SaveBuilder InCustomPath(string filepath, string filename) {
                this.data.filename = filename;
                this.data.filepath = filepath;
                return this;
            }
            public SaveBuilder InCustomFullPath(string filepath) {
                this.data.filename = "";
                if (!data.hasCustomExtension) this.data.fileExtension = "";
                this.data.filepath = filepath;
                return this;
            }
            public SaveBuilder AsJSON(bool prettyFormat = false) {
                if (!data.hasCustomExtension) this.data.fileExtension = ".json";
                this.data.serializeType = prettyFormat ? SerializeType.JSONPRETTY : SerializeType.JSON;
                return this;
            }
            public SaveBuilder AsXML() {
                // todo? any xml opitons
                return this.As(SerializeType.XML);
            }
            public SaveBuilder AsText() {
                if (!data.hasCustomExtension) this.data.fileExtension = ".txt";
                this.data.saveAsBytes = false;
                this.data.serializeType = SerializeType.NONE;
                return this;
            }
            public SaveBuilder AsBinary() {
                this.data.saveAsBytes = true;
                if (!data.hasCustomExtension) this.data.fileExtension = ".bin";
                return this;
            }
            public SaveBuilder As(SerializeType serializeType) {
                this.data.serializeType = serializeType;
                string newExtension = null;
                if (serializeType == SerializeType.JSON || serializeType == SerializeType.JSONPRETTY) {
                    newExtension = ".json";
                    // } else if (serializeType == SerializeType.TEXT) {
                    //     newExtension = ".txt";
                    // } else if (serializeType == SerializeType.BINARY) {
                    //     newExtension = ".bin";
                } else if (serializeType == SerializeType.XML) {
                    newExtension = ".xml";
                } else if (serializeType == SerializeType.YAML) {
                    newExtension = ".yaml"; // ? or .yml
                }
                if (!data.hasCustomExtension && newExtension != null) data.fileExtension = newExtension;
                return this;
            }
            public SaveBuilder CustomExtension(string extension = "txt") {
                this.data.hasCustomExtension = true;
                this.data.fileExtension = "." + extension;
                return this;
            }
            public SaveBuilder CanOverwrite(bool canOverwrite = true) {
                this.data.shouldSaveOverwrite = canOverwrite;
                return this;
            }
            public SaveBuilder AutoIncrement(bool increment = true) {
                this.data.shouldSaveIncrement = increment;
                return this;
            }
            public SaveBuilder CreateDirIfDoesntExist(bool createIfDoesntExist = true) {
                this.data.shouldCreateDirIfDoesntExistOnSave = createIfDoesntExist;
                return this;
            }
            public SaveBuilder SetEncoding(System.Text.Encoding encoding) {
                this.data.encoding = encoding;
                return this;
            }
            public SaveBuilder IsCompressed(bool compressedZip = true, System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) {
                this.data.isCompressedZip = compressedZip;
                this.data.compressionLevel = compressionLevel;
                this.data.saveAsBytes = true; // must be saved as bytes to be compressed
                if (!data.hasCustomExtension) {
                    data.hasCustomExtension = true; // so others dont override
                    data.fileExtension = ".zip";
                }
                return this;
            }
            public SaveBuilder Zip(bool compressedZip = true, System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) {
                return IsCompressed(compressedZip, compressionLevel);
            }
            public SaveBuilder EncryptedWith(string symmetricKey) {
                this.data.useEncryption = true;
                // this.data.encryptionKey = this.data.encoding.GetBytes(key); // ? use our encoding its on the key though
                this.data.encryptionKey = Encoding.UTF8.GetBytes(symmetricKey);
                // Encoding.UTF8.GetChars(data.encryptionKey);
                return this;
            }
            public SaveBuilder EncryptedWith(byte[] symmetricKey) {
                this.data.useEncryption = true;
                this.data.encryptionKey = symmetricKey;
                return this;
            }
            public SaveBuilder Quietly(bool isQuiet=true) {
                this.data.quiet = isQuiet;
                return this;
            }

            static byte[] CompressBytes(byte[] data, System.IO.Compression.CompressionLevel compressionLevel) {
                using (MemoryStream memoryStream = new MemoryStream()) {
                    using (GZipStream gzip = new GZipStream(memoryStream, compressionLevel)) {
                        gzip.Write(data);//,0,data.Length
                    }
                    return memoryStream.ToArray();
                }
            }
            static byte[] DecompressBytes(byte[] zippedData) {
                // Create a GZIP stream with decompression mode.
                // ... Then create a buffer and write into while reading from the GZIP stream.
                using (GZipStream stream = new GZipStream(new MemoryStream(zippedData), CompressionMode.Decompress)) {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream()) {
                        int count = 0;
                        do {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0) {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                        return memory.ToArray();
                    }
                }
            }


            // from https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
            static string EncryptString(string plainText, byte[] key) {
                byte[] iv = new byte[16];
                byte[] array;
                using (Aes aes = Aes.Create()) {
                    aes.Key = key;
                    aes.IV = iv;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream()) {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write)) {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream)) {
                                streamWriter.Write(plainText);
                            }
                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }
            static string DecryptString(string cipherText, byte[] key) {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create()) {
                    aes.Key = key;
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(buffer)) {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream)) {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            // from https://stackoverflow.com/questions/53653510/c-sharp-aes-encryption-byte-array
            static byte[] EncryptBytes(byte[] data, byte[] key) {
                byte[] iv = new byte[16];
                using (var aes = Aes.Create()) {
                    aes.KeySize = 128;
                    aes.BlockSize = 128;
                    aes.Padding = PaddingMode.Zeros;

                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                        return PerformCryptography(data, encryptor);
                    }
                }
            }

            static byte[] DecryptBytes(byte[] data, byte[] key) {
                byte[] iv = new byte[16];
                using (var aes = Aes.Create()) {
                    aes.KeySize = 128;
                    aes.BlockSize = 128;
                    aes.Padding = PaddingMode.Zeros;

                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                        return PerformCryptography(data, decryptor);
                    }
                }
            }

            static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform) {
                using (var ms = new MemoryStream())
                using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write)) {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }

        static string localPath = Application.dataPath + "/";
        static string localDataPath => localPath + "Data/";
        static string persistentPath = Application.persistentDataPath;
        // static string savedataext = ".json";


        public static SaveBuilder StartSave() {
            return new SaveBuilder(true);
        }
        public static SaveBuilder StartLoad() {
            return new SaveBuilder(false);
        }

    }
}