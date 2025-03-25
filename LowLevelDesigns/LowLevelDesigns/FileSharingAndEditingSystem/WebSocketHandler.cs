using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LowLevelDesigns.FileSharingAndEditingSystem
{
    public class WebSocketHandler
    {
        private readonly ConcurrentDictionary<string, WebSocket> _webSockets = new();
        private readonly ConcurrentDictionary<string, string> _documentStates = new();
        private readonly ConcurrentDictionary<string, List<Operation>> _documentOperations = new();
        private readonly ConcurrentDictionary<string, CRDTDocument> _crdtDocuments = new();

        public async Task ListenForMessages(WebSocket webSocket, string documentId)
        {
            var buffer = new byte[1024 * 4];
            while(webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var operation = JsonSerializer.Deserialize<Operation>(message);
                    if(operation != null)
                    {
                        ApplyOperationalTransformation(documentId, operation);
                        ApplyCRDT(documentId, operation);
                        await BroadcastMessage(documentId, message);
                    }
                }
            }
        }

        private async Task BroadcastMessage(string documentId, string message)
        {
            foreach(var socket in _webSockets.Values)
            {
                if(socket.State == WebSocketState.Open)
                {
                    var messageBuffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);                        
                }
            }
        }

        private void ApplyCRDT(string documentId, Operation operation)
        {
            var crdtDoc = _crdtDocuments.GetOrAdd(documentId, new CRDTDocument());
            crdtDoc.ApplyOperation(operation);
        }

        private void ApplyOperationalTransformation(string documentId, Operation operation)
        {
            _documentStates.AddOrUpdate(documentId, "", (key, oldValue) => oldValue);
            _documentOperations.AddOrUpdate(documentId, new List<Operation>(), (key, oldValue) => oldValue);

            var transformedOp = TransformOperation(documentId, operation);
            var currentState = _documentStates[documentId];
            _documentStates[documentId] = currentState.Insert(transformedOp.Position, transformedOp.Text);
            _documentOperations[documentId].Add(transformedOp);
        }

        private Operation TransformOperation(string documentId, Operation newOp)
        {
            foreach (var prevOp in _documentOperations.GetValueOrDefault(documentId, new List<Operation>()))
            {
                if (newOp.Position >= prevOp.Position)
                {
                    newOp.Position += prevOp.Text.Length;
                }
            }
            return newOp;
        }
    }

    public class CRDTDocument
    {
        private readonly ConcurrentDictionary<int, char> document = new();

        public void ApplyOperation(Operation operation)
        {
            for(int i = 0; i < operation.Text.Length; i++)
            {
                document[operation.Position + i] = operation.Text[i];
            }
        }

        public override string ToString()
        {
            var sortedKeys = new List<int>(document.Keys);
            sortedKeys.Sort();
            var sb = new StringBuilder();
            foreach (var key in sortedKeys)
            {
                sb.Append(document[key]);
            }
            return sb.ToString();
        }
    }

    public class Operation
    {
        public string DocumentId { get; set; }
            public int Position { get; set; }
            public string Text { get; set; }
    };
}
