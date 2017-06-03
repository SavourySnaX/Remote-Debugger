﻿/*
 
The MIT License (MIT) 
 
Copyright (c) 2017 Savoury SnaX 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software. 
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE. 

*/

using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace RemoteDebugger
{
    public delegate void CommandResponse(string[] response);
    public class Command
    {
        public Command(string c, CommandResponse cb) { command = c; responseCB = cb; response = new ConcurrentQueue<string>(); }
        public ConcurrentQueue<string> response;
        public string command;
        public CommandResponse responseCB;
    }
    public class TelNetSpec
    {
        ConcurrentQueue<Command> commands;
        public ConcurrentQueue<string> messages;
        TcpClient c;
        NetworkStream s;
        public bool connected;
        string host;
        int port;

        public void UpdateSettings(string ihost,int iport)
        {
            if ((host!=ihost || port!=iport)&& connected)
            {
                CloseConnection();
            }
            host = ihost;
            port = iport;
        }
        public TelNetSpec()
        {
            port = 0;
            host = "";
            connected = false;
            messages = new ConcurrentQueue<string>();
            commands = new ConcurrentQueue<Command>();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ReadConsumer();
            }).Start();
        }

        public void CloseConnection()
        {
            if (connected)
            {
                ImmediateCommand("quit\n");
                s.Close();
                s = null;
                c.Close();
                c = null;
                connected = false;
            }
        }
        void ImmediateCommand(string command)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes(command+"\n");
            try
            {
                s.Write(b, 0, b.Length);
            }
            catch 
            {

            }
        }

        public void SendCommand(string command,CommandResponse cb)
        {
            Command t = new Command(command, cb);
            commands.Enqueue(t);
        }

        public bool IsQueueEmpty()
        {
            return commands.IsEmpty;
        }
        void ReadConsumer()
        {
            Command cCom = null;
            string message = "";
            while (true)
            {
                if (connected == false)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        if (host != "" && port != 0)
                        {
                            c = new TcpClient(host, port);

                            s = c.GetStream();
                            s.ReadTimeout = 50;
                            connected = true;
                        }
                    }
                    catch
                    {
                        if (s != null)
                        {
                            s.Close();
                            s = null;
                        }
                        if (c != null)
                        {
                            c.Close();
                            c = null;
                        }
                        connected = false;
                    }
                    continue;
                }
                int r = -1;
                try
                {
                    r = s.ReadByte();
                }
                catch (System.NullReferenceException)
                {
                    continue;
                }
                catch (System.ObjectDisposedException)
                {
                    continue;
                }
                catch (System.IO.IOException)
                {
                    if (cCom==null)
                    {
                        // nothing to do
                    }
                    else
                    {
                        cCom.responseCB(cCom.response.ToArray());
                        cCom = null;
                    }
                    // Ready for next command?
                    if (commands.TryDequeue(out cCom))
                    {
                        byte[] b = System.Text.Encoding.ASCII.GetBytes(cCom.command + "\n");
                        try
                        {
                            s.Write(b, 0, b.Length);
                        }
                        catch
                        {
                            //swallow for now
                        }
                        message = "";

                        if (cCom.responseCB==null)
                        {
                            cCom = null;
                        }
                    }
                    continue;
                }
                if (r != -1)
                {
                    byte a = (byte)r;
                    if (a == '\n')
                    {
                        if (cCom != null)
                        {
                            cCom.response.Enqueue(message);
                        }
                        else
                        {
                            messages.Enqueue(message);
                        }
                        message = "";
                    }
                    else
                    {
                        message += System.Text.Encoding.ASCII.GetString(new byte[] { a });
                    }
                }
            }
        }
    }
}
