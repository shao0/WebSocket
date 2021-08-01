using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketTest
{
    public class Analysis

    {
        /// <summary>
        /// 数据包长度
        /// </summary>
        public int PacketLength { get; set; }

        /// <summary>
        /// 数据包头部长度（固定为 16）
        /// </summary>
        public int HeaderLength { get; set; }

        /// <summary>
        /// 协议版本
        /// <para> 0 JSON	JSON纯文本，可以直接通过 JSON.stringify 解析</para>
        ///	<para> 1 Int 32 Big Endian   Body 内容为房间人气值</para>
        ///	<para> 2 Buffer 压缩过的 Buffer，Body 内容需要用zlib.inflate解压出一个新的数据包，然后从数据包格式那一步重新操作一遍</para>
        ///	<para> 3 Buffer 压缩信息, 需要brotli解压, 然后从数据包格式 那一步重新操作一遍</para>
        /// </summary>
        public int ProtocolVersion { get; set; }

        /// <summary>
        /// 操作类型
        /// <para>2	客户端	(空)	心跳	不发送心跳包，70 秒之后会断开连接，通常每 30 秒发送 1 次</para>
        /// <para>3	服务器 Int 32 Big Endian   心跳回应 Body 内容为房间人气值</para>
        /// <para>5	服务器 JSON    通知 弹幕、广播等全部信息</para>
        /// <para>7	客户端 JSON    进房 WebSocket 连接成功后的发送的第一个数据包，发送要进入房间 ID</para>
        /// <para>8	服务器(空) 进房回应</para>
        /// </summary>
        public int Operation { get; set; }

        /// <summary>
        /// 序列Id
        /// </summary>
        public int SequenceId { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        public byte[] Body { get; set; }
    }
}
