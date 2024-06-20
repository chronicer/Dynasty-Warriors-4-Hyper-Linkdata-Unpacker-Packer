using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DW_4_Unpacker
{
    public partial class Form1 : Form
    {

        private static readonly string aMediaDataEtcMd = "D:\\Games\\Dinasty Warriors 4\\media\\data\\etc\\mdata.bin"; // Замените на реальный путь к файлу
        private static readonly string aMediaDataBin = "D:\\Games\\Dinasty Warriors 4\\media\\linkdata.bin"; // Замените на реальный путь к файлу
        private static readonly int[] m_nFileNamesMaybe = new int[2364 * 2]; // Массив для хранения данных из файла
        private const string SettingsFilePath = "settings.txt";

        private string[] filePaths; // Массив путей к файлам в архиве

        public static void UnpackFiles(string mdataPath, string source, string outputDirectory, int numberOfFilesToUnpack)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // read mdata.bin
            ReadMediaDataBin(mdataPath);

           
            using (FileStream sourceFile = new FileStream(source, FileMode.Open, FileAccess.Read))
            {
                // read file paths from fileList.txt
                string[] filePaths = File.ReadAllLines("fileList.txt");

                // unpack files
                for (int i = 0; i < numberOfFilesToUnpack; i++)
                {
                    // check file start offset (for example - 0x5B * 0x800 in mdata.bin)
                    int fileStartOffset = m_nFileNamesMaybe[i * 2] * 0x800;
                    int fileEndOffset = fileStartOffset + m_nFileNamesMaybe[i * 2 + 1];

                    // check if file is not empty
                    if (fileEndOffset > fileStartOffset)
                    {
                        // path to file from list
                        string outputFilePath = Path.Combine(outputDirectory, filePaths[i]);

                        // create directiories if need
                        string outputDirectory2 = Path.GetDirectoryName(outputFilePath);
                        if (!Directory.Exists(outputDirectory2))
                        {
                            Directory.CreateDirectory(outputDirectory2);
                        }

                       
                        using (FileStream outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                        {
                            // Читаем данные из исходного файла и записываем в выходной файл
                            sourceFile.Seek(fileStartOffset, SeekOrigin.Begin);
                            byte[] buffer = new byte[fileEndOffset - fileStartOffset];
                            sourceFile.Read(buffer, 0, buffer.Length);
                            outputFile.Write(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        // if file is empty, then create "file_i.bin"
                        string outputFileName = Path.Combine(outputDirectory, $"file_{i}.bin");
                        File.Create(outputFileName).Dispose();
                    }
                }
            }
        }

        // Reading mdata.bin offsets for files
        public static int ReadMediaDataBin(string mdataPath)
        {

                // (rb - read binary)
                using (FileStream fileStream = new FileStream(mdataPath, FileMode.Open, FileAccess.Read))
                {
                    // skip 16 bytes
                    fileStream.Seek(16, SeekOrigin.Begin);


                    BinaryReader reader = new BinaryReader(fileStream);
                    for (int j = 0; j < 2364; j++) //2363 - count of files
                    {
                        // read 16 bytes to buffer
                        byte[] buffer = reader.ReadBytes(16);

                        // Извлекаем нужные данные из буфера
                        int value1 = BitConverter.ToInt32(buffer, 0); // Buffer[0]
                        int value2 = BitConverter.ToInt32(buffer, 8); // Buffer[2]

                        // write values to array
                        m_nFileNamesMaybe[j * 2] = value1;
                        m_nFileNamesMaybe[j * 2 + 1] = value2;
                    }
                }
            return 1;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox3.Text != "")
            {
                UnpackFiles(textBox1.Text, textBox2.Text, textBox3.Text, 2364);
            }
            else
            {
                MessageBox.Show("Please, select mdata.bin file, linkdata.bin file and folder for unpacked files!");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filePaths = File.ReadAllLines("fileList.txt");

            foreach (string filePath in filePaths)
            {
                listBox1.Items.Add(filePath);
            }

            LoadSettings();

        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string[] lines = File.ReadAllLines(SettingsFilePath);
                if (lines.Length > 0)
                    textBox1.Text = lines[0];
                if (lines.Length > 1)
                    textBox2.Text = lines[1];
                if (lines.Length > 2)
                    textBox3.Text = lines[2];
            }
        }

        private void SaveSettings()
        {
            string[] lines = new string[] { textBox1.Text, textBox2.Text, textBox3.Text };
            File.WriteAllLines(SettingsFilePath, lines);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();


            openFileDialog.Title = "Select mdata.bin file";

          
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
             
                string filePath = openFileDialog.FileName;

                textBox1.Text = filePath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();


            openFileDialog.Title = "Select linkdata.bin file";

           
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                string filePath = openFileDialog.FileName;

                textBox2.Text = filePath;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();


            folderBrowserDialog.Description = "Select folder for unpacked files";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {

                string folderPath = folderBrowserDialog.SelectedPath;

                textBox3.Text = folderPath;
            }
        }

        private void ReplaceFileInArchive(int fileIndex, string newFilePath)
        {
            // calculation file start offset (for example - 0x5B * 0x800 in mdata.bin)
            int fileStartOffset = m_nFileNamesMaybe[fileIndex * 2] * 0x800;

            //file end offset
            int fileEndOffset = fileStartOffset + m_nFileNamesMaybe[fileIndex * 2 + 1];

            // Проверка, что файл не пустой
            if (fileEndOffset > fileStartOffset)
            {
                // Чтение данных из нового файла
                byte[] fileData = File.ReadAllBytes(newFilePath);

                // Запись данных в архив
                using (FileStream archiveFile = new FileStream(textBox2.Text, FileMode.Open, FileAccess.ReadWrite))
                {
                    archiveFile.Seek(fileStartOffset, SeekOrigin.Begin);
                    archiveFile.Write(fileData, 0, fileData.Length);
                }
                MessageBox.Show("File is replaced!");
            }
        }

        // not used func
        private void UpdateFileIndexes(int fileIndex, int sizeDifference)
        {
            
            m_nFileNamesMaybe[fileIndex * 2 + 1] += sizeDifference;

            
            for (int i = fileIndex + 1; i < m_nFileNamesMaybe.Length / 2; i++)
            {
                m_nFileNamesMaybe[i * 2] += sizeDifference / 0x800;
                m_nFileNamesMaybe[i * 2 + 1] += sizeDifference;
            }

           
            using (FileStream indexFile = new FileStream(textBox1.Text, FileMode.Open, FileAccess.ReadWrite))
            {
                
                indexFile.Seek(16, SeekOrigin.Begin);

                
                BinaryWriter writer = new BinaryWriter(indexFile);
                for (int i = 0; i < m_nFileNamesMaybe.Length; i++)
                {
                   
                    int valueToWrite = (int)(m_nFileNamesMaybe[i] & 0x00FFFFFF);
                    writer.Write(valueToWrite);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                //read indexes from mdata.bin
                ReadMediaDataBin(textBox1.Text);

                // if file selected in listbox!
                if (listBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Please, select file in file list!");
                    return;
                }

           
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select file for replacing";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string newFilePath = openFileDialog.FileName;

                    // replace file in linkdata.bin
                    ReplaceFileInArchive(listBox1.SelectedIndex, newFilePath);

                }
            }
            else
            {
                MessageBox.Show("Please, select path to mdata.bin and linkdata.bin!!!");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }
    }
}
