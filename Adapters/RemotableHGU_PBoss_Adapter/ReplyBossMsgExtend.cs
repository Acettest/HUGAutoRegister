using System;
using System.Collections.Generic;
using System.Text;
using HGU.Idl;

namespace RemotableHGU_PBoss_Adapter
{
    public enum ReplyType
    {
        NormalTask, 
        RollbackTask, 
        NetInterrupt
    }

    public class ReplyBossMsgExtend
    {
        private ReplyBossMsg replyBoss;
        private ReplyType type;

        public ReplyBossMsg ReplyBoss
        {
            get { return replyBoss; }
        }

        public ReplyType Type
        {
            get { return type; }
        }

        public ReplyBossMsgExtend(ReplyBossMsg replyBoss, ReplyType type)
        {
            this.replyBoss = replyBoss;
            this.type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return CompareMembers(this, obj as ReplyBossMsgExtend);
        }

        public static bool CompareMembers(ReplyBossMsgExtend left, ReplyBossMsgExtend right)
        {
            if (left == null || right == null)
                return false;
            if ((left.ReplyBoss.TaskID != right.ReplyBoss.TaskID) || (left.Type != right.Type))
                return false;
            return true;
        }
    }
}
