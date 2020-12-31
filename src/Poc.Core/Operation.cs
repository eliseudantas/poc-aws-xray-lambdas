using System;

namespace Poc.Core
{
    public class Operation
    {
        public int OperationId { get; set; }
        public string RecordId { get; set; }
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Operation other &&
                   OperationId == other.OperationId &&
                   RecordId == other.RecordId &&
                   Description == other.Description;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OperationId, RecordId, Description);
        }
    }
}
