using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HL7Reciver.Models;
using Mahas.Helpers;
using Newtonsoft.Json;

namespace HL7Reciver
{
    class Program
    {
        public static string ConString { get; set; }

        public static async void SaveToDb(List<string> data, List<string> sendBack, bool isSuccess)
        {
            var strData = string.Join("", data);
            var strSendBack = string.Join("", data);

            using var s = new MahasConnection(ConString);
            s.OpenTransaction();
            await s.Insert(new ListenerModel()
            {
                Pesan = strData,
                SendBack = strSendBack,
                Tanggal = DateTime.Now,
                isSuccess = isSuccess
            }, true);
            s.CommitTransaction();
        }

        public static void Main()
        {
            // environment
            EnvModel env;
            using (var r = new StreamReader("env.json"))
            {
                string json = r.ReadToEnd();
                env = JsonConvert.DeserializeObject<EnvModel>(json);
                ConString = env.ConnectionString;
            }

            TcpListener server = null;
            try
            {
                var END_OF_BLOCK = '\u001c';
                var START_OF_BLOCK = '\u000b';
                var CARRIAGE_RETURN = (char)13;

                var testHl7MessageToTransmit = new StringBuilder();
                testHl7MessageToTransmit.Append(START_OF_BLOCK);
                testHl7MessageToTransmit.Append(string.Join(CARRIAGE_RETURN, env.SendBack));
                testHl7MessageToTransmit.Append(END_OF_BLOCK);

                // port
                Console.Write("Port : (default 1234) ");
                var _port = Console.ReadLine();
                int port = 1234;
                if (!string.IsNullOrEmpty(_port))
                {
                    _ = int.TryParse(_port, out port);
                }
                Console.WriteLine($"Port : {port}");

                // start listener
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                while (true)
                {
                    byte[] bytes = new byte[256];
                    var data = new List<string>();
                    var sendBack = new List<string>();

                    Console.Write("Waiting for a connection... ");
                    TcpClient client = server.AcceptTcpClient();

                    Console.WriteLine("Connected!");
                    NetworkStream stream = client.GetStream();

                    int? i;
                    bool isSuccess = true;
                    try
                    {
                        isSuccess = true;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var _data = Encoding.ASCII.GetString(bytes, 0, i.Value);
                            var _sendBack = _data;

                            var datas = _data.Trim().Split("|");
                            if (datas.Length >= env.ControlIDIndex - 1)
                            {
                                if (datas[0] == "MSH")
                                {
                                    var id = datas[env.ControlIDIndex - 1];
                                    _sendBack = testHl7MessageToTransmit.ToString();
                                    _sendBack = _sendBack.Replace("@ControlID", id);
                                }
                            }

                            byte[] msg = Encoding.ASCII.GetBytes(_sendBack);
                            stream.Write(msg, 0, msg.Length);
                            data.Add(_data);
                            sendBack.Add(_sendBack);

                            // clear
                            msg = null;
                            _data = null;
                            _sendBack = null;
                        }
                    }
                    catch {
                        isSuccess = false;
                    }

                    client.Close();
                    stream.Close();

                    // save to Db
                    SaveToDb(data, sendBack, isSuccess);
                    Console.WriteLine($"Save to DB");

                    // clear
                    data = null;
                    sendBack = null;
                    i = null;
                    bytes = null;
                    client = null;
                    stream = null;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
