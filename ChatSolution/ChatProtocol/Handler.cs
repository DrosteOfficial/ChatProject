﻿using System.Net.Sockets;
using ChatProtocol;
using ChatProtocol.Writer;
using ConsoleApp1;
using Server;

public class Handler
{
    public delegate void MessagePacketHandler(IPacket packet, Handler handler);
    public delegate void LoginRequestHandler(IPacket packet, Handler handler);

    public delegate void SendLoginResponceHandler(IPacket packet);

    public Guid Guid = Guid.NewGuid();
    public  LoginRequest LoginRequest;
    public  LoginResponse LoginResponse;
    public  MessagePacket MessagePacket;
    public  NetworkStream NetworkStream;
    public  PacketReader PacketReader;
    public  PacketWriter PacketWriter;
    public RegisterRequest RegisterRequest;
    public RegisterResponse RegisterResponse;
    public IPacket? PacketRead;
    private Dictionary<int, Delegate> PacketHandlers;
    
    public Handler()
    {
        PacketWriter = new PacketWriter(this);
        PacketReader = new PacketReader(this);
        LoginResponse = new LoginResponse
        {
            Message = ""
        };
        LoginRequest = new LoginRequest
        {
            Username = "",
            Password = ""
        };
        MessagePacket = new MessagePacket
        {
            Message = ""
        };
        RegisterRequest = new RegisterRequest
        {
            Username = "",
            Password = "",
            Email = ""
        };
        RegisterResponse = new RegisterResponse
        {
            Message = ""
        };
        PacketHandlers = new Dictionary<int, Delegate>()
        {
            { 0, (MessagePacketHandler)HandleMessagePacket },
            { 1, (LoginRequestHandler)HandleLoginRequest },
            { 3, (LoginRequestHandler)HandleRegisterRequest },
            { 4, (MessagePacketHandler)HandleRegisterResponse }
        };
        PacketRead = MessagePacket;


    }
    public void Handle()
    {
        Console.WriteLine("Handling packet");
         if (PacketRead == null)
        {
            return;
        }
        
        if(!PacketHandlers.ContainsKey(PacketReader.packetContainer.PacketId())? true : false)
        {
            Console.WriteLine("Packet id not found");
            return;
        }

        foreach (KeyValuePair<int,Delegate>  packetHandler in PacketHandlers)
        {
            if (packetHandler.Key == PacketReader.packetContainer.PacketId())
            {
                packetHandler.Value.DynamicInvoke(PacketReader.packetContainer, this);
            }
        }
    }

    // Metody do obsługi różnych typów pakietów
    private void HandleMessagePacket(IPacket packet , Handler handler)
    {
        var packat1 = PacketReader.ObjectRead as MessagePacket;
        Console.WriteLine(packat1.Message);
    }

    private void HandleLoginRequest(IPacket packet , Handler handler)
    {
        LoginRequest? packet1 = PacketReader.ObjectRead as LoginRequest;
        handler.LoginResponse.Guid = handler.Guid;
        if (packet1 == null)
        {
            handler.LoginResponse.Message = "Login failed null packet!";
            handler.PacketWriter.WritePacket(handler.LoginResponse);
            handler.PacketWriter.Flush(handler.NetworkStream);
            return;
        }
       
        handler.LoginResponse.Message = PacketMethods.ReturnDbQuerryAnswer(packet1).Result;
        handler.PacketWriter.WritePacket(handler.LoginResponse);
        handler.PacketWriter.Flush(handler.NetworkStream);
    }
    
    public string HandleLoginResponse(IPacket packet)
    {
        var packet1 = (LoginResponse)packet;
        Console.WriteLine(packet1.Message);
        return packet1.Message;
    }
    private void HandleRegisterRequest(IPacket packet , Handler handler)
    {
        RegisterRequest? packet1 = PacketReader.ObjectRead as RegisterRequest;
        handler.RegisterResponse.Guid = handler.Guid;
        if (packet1 == null)
        {
            handler.RegisterResponse.Message = "Registration failed null packet!";
            handler.PacketWriter.WritePacket(handler.RegisterResponse);
            handler.PacketWriter.Flush(handler.NetworkStream);
            return;
            
        }
        handler.RegisterResponse.Message = PacketMethods.AddUser(packet1).Result;
        handler.PacketWriter.WritePacket(handler.RegisterResponse);

        
        handler.PacketWriter.Flush(handler.NetworkStream);
  
    }
    private void HandleRegisterResponse(IPacket packet , Handler handler)
    {
        Console.WriteLine("Register response");
        RegisterResponse? packet1 = PacketReader.ObjectRead as RegisterResponse;
        if (packet1 == null)
        {
            Console.WriteLine("Registration failed null packet!");
        }
        Console.WriteLine(packet1.Message);
    }
    
    
}