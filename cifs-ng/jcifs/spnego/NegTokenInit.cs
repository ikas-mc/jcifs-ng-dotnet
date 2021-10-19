using System;
using System.IO;
using cifs_ng.lib.ext;
using jcifs.util;
using Org.BouncyCastle.Asn1;

/* jcifs smb client library in Java
 * Copyright (C) 2004  "Michael B. Allen" <jcifs at samba dot org>
 *                   "Eric Glass" <jcifs at samba dot org>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace jcifs.spnego
{
    /// <summary>
    /// SPNEGO initial token
    /// </summary>
    public class NegTokenInit : SpnegoToken
    {
        public const int DELEGATION = 0x80;
        public const int MUTUAL_AUTHENTICATION = 0x40;
        public const int REPLAY_DETECTION = 0x20;
        public const int SEQUENCE_CHECKING = 0x10;
        public const int ANONYMITY = 0x08;
        public const int CONFIDENTIALITY = 0x04;
        public const int INTEGRITY = 0x02;

        private static readonly DerObjectIdentifier SPNEGO_OID = new DerObjectIdentifier(SpnegoConstants.SPNEGO_MECHANISM);

        private DerObjectIdentifier[] mechanisms;

        private int contextFlags;


        public NegTokenInit()
        {
        }


        public NegTokenInit(DerObjectIdentifier[] mechanisms, int contextFlags, byte[] mechanismToken, byte[] mechanismListMIC)
        {
            setMechanisms(mechanisms);
            setContextFlags(contextFlags);
            setMechanismToken(mechanismToken);
            setMechanismListMIC(mechanismListMIC);
        }


		/// throws java.io.IOException
        public NegTokenInit(byte[] token)
        {
            parse(token);
        }


        public virtual int getContextFlags()
        {
            return this.contextFlags;
        }


        public virtual void setContextFlags(int contextFlags)
        {
            this.contextFlags = contextFlags;
        }


        public virtual bool getContextFlag(int flag)
        {
            return (getContextFlags() & flag) == flag;
        }


        public virtual void setContextFlag(int flag, bool value)
        {
            setContextFlags(value ? (getContextFlags() | flag) : (int) (getContextFlags() & (0xffffffff ^ flag)));
        }


        public virtual DerObjectIdentifier[] getMechanisms()
        {
            return this.mechanisms;
        }


        public virtual void setMechanisms(DerObjectIdentifier[] mechanisms)
        {
            this.mechanisms = mechanisms;
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= java.lang.Object#toString() </seealso>
        public override string ToString()
        {
            string mic = null;
            if (this.getMechanismListMIC() != null)
            {
                mic = Hexdump.toHexString(this.getMechanismListMIC(), 0, this.getMechanismListMIC().Length);
            }

            return $"NegTokenInit[flags={this.getContextFlags()},mechs={this.getMechanisms()?.joinToString()},mic={mic}]";
        }


        public override byte[] toByteArray()
        {
            try
            {
                Asn1EncodableVector fields = new Asn1EncodableVector();
                DerObjectIdentifier[] mechs = getMechanisms();
                if (mechs != null)
                {
                    Asn1EncodableVector vector = new Asn1EncodableVector();
                    for (int i = 0; i < mechs.Length; i++)
                    {
                        vector.Add(mechs[i]);
                    }

                    fields.Add(new DerTaggedObject(true, 0, new DerSequence(vector)));
                }

                int ctxFlags = getContextFlags();
                if (ctxFlags != 0)
                {
                    fields.Add(new DerTaggedObject(true, 1, new DerInteger(ctxFlags))); //TODO 
                }

                byte[] mechanismToken = getMechanismToken();
                if (mechanismToken != null)
                {
                    fields.Add(new DerTaggedObject(true, 2, new DerOctetString(mechanismToken)));
                }

                byte[] mechanismListMIC = getMechanismListMIC();
                if (mechanismListMIC != null)
                {
                    fields.Add(new DerTaggedObject(true, 3, new DerOctetString(mechanismListMIC)));
                }

                Asn1EncodableVector ev = new Asn1EncodableVector();
                ev.Add(SPNEGO_OID);
                ev.Add(new DerTaggedObject(true, 0, new DerSequence(fields)));
                MemoryStream collector = new MemoryStream();
                DerOutputStream Der = new DerOutputStream(collector);
                DerApplicationSpecific DerApplicationSpecific = new DerApplicationSpecific(0, ev);
                Der.WriteObject(DerApplicationSpecific);
                return collector.ToArray();
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }


		/// throws java.io.IOException
        protected internal override void parse(byte[] token)
        {
            using (Asn1InputStream @is = new Asn1InputStream(token))
            {
                DerApplicationSpecific constructed = (DerApplicationSpecific) @is.ReadObject();
                if (constructed == null || !constructed.IsConstructed())
                {
                    throw new IOException("Malformed SPNEGO token " + constructed + (constructed != null ? " " + constructed.IsConstructed() + " " + constructed.ApplicationTag : ""));
                }

                using (Asn1InputStream Der = new Asn1InputStream(constructed.GetContents()))
                {
                    DerObjectIdentifier spnego = (DerObjectIdentifier) Der.ReadObject();
                    if (!SPNEGO_OID.Equals(spnego))
                    {
                        throw new IOException("Malformed SPNEGO token, OID " + spnego);
                    }

                    Asn1TaggedObject tagged = (Asn1TaggedObject) Der.ReadObject();
                    if (tagged.TagNo != 0)
                    {
                        throw new IOException("Malformed SPNEGO token: tag " + tagged.TagNo + " " + tagged);
                    }

                    Asn1Sequence sequence = Asn1Sequence.GetInstance(tagged, true);
                    var fields = sequence.GetEnumerator();
                    while (fields.MoveNext())
                    {
                        tagged = (Asn1TaggedObject) fields.Current;
                        switch (tagged.TagNo)
                        {
                            case 0:
                                sequence = Asn1Sequence.GetInstance(tagged, true);
                                DerObjectIdentifier[] mechs = new DerObjectIdentifier[sequence.Count];
                                for (int i = mechs.Length - 1; i >= 0; i--)
                                {
                                    mechs[i] = (DerObjectIdentifier) sequence[i];
                                }

                                setMechanisms(mechs);
                                break;
                            case 1:
                                DerInteger ctxFlags = DerInteger.GetInstance(tagged, true);
                                //TODO 
                                setContextFlags((int) (ctxFlags.Value.LongValue) & 0xff);
                                break;
                            case 2:
                                Asn1OctetString mechanismToken = Asn1OctetString.GetInstance(tagged, true);
                                setMechanismToken(mechanismToken.GetOctets());
                                break;

                            case 3:
                                if (!(tagged.GetObject() is DerOctetString))
                                {
                                    break;
                                }

                                //TODO
                                Asn1OctetString mechanismListMIC2 = Asn1OctetString.GetInstance(tagged, true);
                                setMechanismListMIC(mechanismListMIC2.GetOctets());
                                break;
                            case 4:
                                Asn1OctetString mechanismListMIC = Asn1OctetString.GetInstance(tagged, true);
                                setMechanismListMIC(mechanismListMIC.GetOctets());
                                break;
                            default:
                                throw new IOException("Malformed token field.");
                        }
                    }
                }
            }
        }
    }
}