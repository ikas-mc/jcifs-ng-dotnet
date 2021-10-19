using System;
using System.IO;
using cifs_ng.lib.ext;
using Org.BouncyCastle.Asn1;

/*
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
namespace jcifs.smb
{
    using Logger = org.slf4j.Logger;
    using LoggerFactory = org.slf4j.LoggerFactory;
    using CIFSException = jcifs.CIFSException;
    using Configuration = jcifs.Configuration;
    using NegTokenInit = jcifs.spnego.NegTokenInit;
    using NegTokenTarg = jcifs.spnego.NegTokenTarg;
    using SpnegoException = jcifs.spnego.SpnegoException;
    using SpnegoToken = jcifs.spnego.SpnegoToken;
    using Hexdump = jcifs.util.Hexdump;


    /// <summary>
    /// This class used to wrap a <seealso cref="SSPContext"/> to provide SPNEGO feature.
    /// 
    /// @author Shun
    /// 
    /// </summary>
    internal class SpnegoContext : SSPContext
    {
        private static readonly Logger log = LoggerFactory.getLogger(typeof(SpnegoContext));

        private static DerObjectIdentifier SPNEGO_MECH_OID;

        static SpnegoContext()
        {
            try
            {
                SPNEGO_MECH_OID = new DerObjectIdentifier("1.3.6.1.5.5.2");
            }
            catch (System.ArgumentException e)
            {
                log.error("Failed to initialize OID", e);
            }
        }

        private SSPContext mechContext;

        private bool firstResponse = true;
        private bool completed;

        private DerObjectIdentifier[] mechs;
        private DerObjectIdentifier selectedMech;
        private DerObjectIdentifier[] remoteMechs;

        private bool disableMic;
        private bool requireMic;


        /// <summary>
        /// Instance a <code>SpnegoContext</code> object by wrapping a <seealso cref="SSPContext"/>
        /// with the same mechanism this <seealso cref="SSPContext"/> used.
        /// </summary>
        /// <param name="source">
        ///            the <seealso cref="SSPContext"/> to be wrapped </param>
        internal SpnegoContext(Configuration config, SSPContext source) : this(config, source, source.getSupportedMechs())
        {
        }


        /// <summary>
        /// Instance a <code>SpnegoContext</code> object by wrapping a <seealso cref="SSPContext"/>
        /// with specified mechanism.
        /// </summary>
        /// <param name="source">
        ///            the <seealso cref="SSPContext"/> to be wrapped </param>
        /// <param name="mech">
        ///            the mechanism is being used for this context. </param>
        internal SpnegoContext(Configuration config, SSPContext source, DerObjectIdentifier[] mech)
        {
            this.mechContext = source;
            this.mechs = mech;
            this.disableMic = !config.isEnforceSpnegoIntegrity() && config.isDisableSpnegoIntegrity();
            this.requireMic = config.isEnforceSpnegoIntegrity();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#getSupportedMechs() </seealso>
        public virtual DerObjectIdentifier[] getSupportedMechs()
        {
            return new DerObjectIdentifier[] {SPNEGO_MECH_OID}; 
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#getFlags() </seealso>
        public virtual int getFlags()
        {
             return this.mechContext.getFlags(); 
        }


        public virtual bool isSupported(DerObjectIdentifier mechanism)
        {
            // prevent nesting
            return false;
        }


        /// <summary>
        /// Determines what mechanism is being used for this context.
        /// </summary>
        /// <returns> the Oid of the mechanism being used </returns>
        internal virtual DerObjectIdentifier[] getMechs()
        {
            return this.mechs;
        }
        
        internal virtual void getMechs(DerObjectIdentifier[] value)
        {
            this.mechs = value;
        }


        /// <returns> the mechanisms announced by the remote end </returns>
        internal virtual DerObjectIdentifier[] getRemoteMechs()
        {
            return this.remoteMechs;
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#getNetbiosName() </seealso>
        public virtual string getNetbiosName()
        {
            return null; 
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#getSigningKey() </seealso>
		/// throws jcifs.CIFSException
        public virtual byte[] getSigningKey()
        {
           return this.mechContext.getSigningKey();
        }


        /// <summary>
        /// Initialize the GSSContext to provide SPNEGO feature.
        /// </summary>
        /// <param name="inputBuf"> </param>
        /// <param name="offset"> </param>
        /// <param name="len"> </param>
        /// <returns> response token </returns>
		/// throws jcifs.CIFSException
        public virtual byte[] initSecContext(byte[] inputBuf, int offset, int len)
        {
            SpnegoToken resp;
            if (this.completed)
            {
                throw new CIFSException("Already complete");
            }
            else if (len == 0)
            {
                resp = initialToken();
            }
            else
            {
                resp = negotitate(inputBuf, offset, len);
            }

            if (resp == null)
            {
                return null;
            }

            return resp.toByteArray();
        }


		/// throws jcifs.CIFSException
        private SpnegoToken negotitate(byte[] inputBuf, int offset, int len)
        {
            SpnegoToken spToken = getToken(inputBuf, offset, len);
            byte[] inputToken = null;
            if (spToken is NegTokenInit)
            {
                NegTokenInit tinit = (NegTokenInit) spToken;
                DerObjectIdentifier[] rm = tinit.getMechanisms();
                this.remoteMechs = rm;
                DerObjectIdentifier prefMech = rm[0];
                // only use token if the optimistic mechanism is supported
                if (this.mechContext.isSupported(prefMech))
                {
                    inputToken = tinit.getMechanismToken();
                }
                else
                {
                    DerObjectIdentifier found = null;
                    foreach (DerObjectIdentifier mech in rm)
                    {
                        if (this.mechContext.isSupported(mech))
                        {
                            found = mech;
                            break;
                        }
                    }

                    if (found == null)
                    {
                        throw new SmbException("Server does advertise any supported mechanism");
                    }
                }
            }
            else if (spToken is NegTokenTarg)
            {
                NegTokenTarg targ = (NegTokenTarg) spToken;

                if (this.firstResponse)
                {
                    if (!this.mechContext.isSupported(targ.getMechanism()))
                    {
                        throw new SmbException("Server chose an unsupported mechanism " + targ.getMechanism());
                    }

                    this.selectedMech = targ.getMechanism();
                    if (targ.getResult() == NegTokenTarg.REQUEST_MIC)
                    {
                        this.requireMic = true;
                    }

                    this.firstResponse = false;
                }
                else
                {
                    if (targ.getMechanism() != null && !targ.getMechanism().Equals(this.selectedMech))
                    {
                        throw new SmbException("Server switched mechanism");
                    }
                }

                inputToken = targ.getMechanismToken();
            }
            else
            {
                throw new SmbException("Invalid token");
            }

            if (spToken is NegTokenTarg && this.mechContext.isEstablished())
            {
                // already established, but server hasn't completed yet
                NegTokenTarg targ = (NegTokenTarg) spToken;

                if (targ.getResult() == NegTokenTarg.ACCEPT_INCOMPLETE && targ.getMechanismToken() == null && targ.getMechanismListMIC() != null)
                {
                    // this indicates that mechlistMIC is required by the server
                    verifyMechListMIC(targ.getMechanismListMIC());
                    return new NegTokenTarg(NegTokenTarg.UNSPECIFIED_RESULT, null, null, calculateMechListMIC());
                }
                else if (targ.getResult() != NegTokenTarg.ACCEPT_COMPLETED)
                {
                    throw new SmbException("SPNEGO negotiation did not complete");
                }

                verifyMechListMIC(targ.getMechanismListMIC());
                this.completed = true;
                return null;
            }

            if (inputToken == null)
            {
                return initialToken();
            }

            byte[] mechMIC = null;
            byte[] responseToken = this.mechContext.initSecContext(inputToken, 0, inputToken.Length);

            if (spToken is NegTokenTarg)
            {
                NegTokenTarg targ = (NegTokenTarg) spToken;
                if (targ.getResult() == NegTokenTarg.ACCEPT_COMPLETED && this.mechContext.isEstablished())
                {
                    // server sent final token
                    verifyMechListMIC(targ.getMechanismListMIC());
                    if (!this.disableMic || this.requireMic)
                    {
                        mechMIC = calculateMechListMIC();
                    }

                    this.completed = true;
                }
                else if (this.mechContext.isMICAvailable() && (!this.disableMic || this.requireMic))
                {
                    // we need to send our final data
                    mechMIC = calculateMechListMIC();
                }
                else if (targ.getResult() == NegTokenTarg.REJECTED)
                {
                    throw new SmbException("SPNEGO mechanism was rejected");
                }
            }

            if (responseToken == null && this.mechContext.isEstablished())
            {
                return null;
            }

            return new NegTokenTarg(NegTokenTarg.UNSPECIFIED_RESULT, null, responseToken, mechMIC);
        }


		/// throws jcifs.CIFSException
        private byte[] calculateMechListMIC()
        {
            if (!this.mechContext.isMICAvailable())
            {
                return null;
            }

            DerObjectIdentifier[] lm = this.mechs;
            byte[] ml = encodeMechs(lm);
            byte[] mechanismListMIC = this.mechContext.calculateMIC(ml);
            if (log.isDebugEnabled())
            {
                log.debug("Out Mech list " + lm?.joinToString());
                log.debug("Out Mech list encoded " + Hexdump.toHexString(ml));
                log.debug("Out Mech list MIC " + Hexdump.toHexString(mechanismListMIC));
            }

            return mechanismListMIC;
        }


		/// throws jcifs.CIFSException
        private void verifyMechListMIC(byte[] mechanismListMIC)
        {
            if (this.disableMic)
            {
                return;
            }

            // No MIC verification if not present and not required
            // or if the chosen mechanism is our preferred one
            if ((mechanismListMIC == null || !this.mechContext.supportsIntegrity()) && this.requireMic && !this.mechContext.isPreferredMech(this.selectedMech))
            {
                throw new CIFSException("SPNEGO integrity is required but not available");
            }

            // otherwise we ignore the absence of a MIC
            if (!this.mechContext.isMICAvailable() || mechanismListMIC == null)
            {
                return;
            }

            try
            {
                DerObjectIdentifier[] lm = this.mechs;
                byte[] ml = encodeMechs(lm);
                if (log.isInfoEnabled())
                {
                    log.debug("In Mech list " + lm?.joinToString());
                    log.debug("In Mech list encoded " + Hexdump.toHexString(ml));
                    log.debug("In Mech list MIC " + Hexdump.toHexString(mechanismListMIC));
                }

                this.mechContext.verifyMIC(ml, mechanismListMIC);
            }
            catch (CIFSException e)
            {
                throw new CIFSException("Failed to verify mechanismListMIC", e);
            }
        }


        /// <param name="mechs">
        /// @return </param>
        /// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
        private static byte[] encodeMechs(DerObjectIdentifier[] mechs)
        {
            try
            {
                MemoryStream bos = new MemoryStream();
                DerOutputStream dos = new DerOutputStream(bos);
                dos.WriteObject(new DerSequence(mechs));
                dos.Dispose();
                return bos.ToArray();
            }
            catch (IOException e)
            {
                throw new CIFSException("Failed to encode mechList", e);
            }
        }


		/// throws jcifs.CIFSException
        private SpnegoToken initialToken()
        {
            byte[] mechToken = this.mechContext.initSecContext(new byte[0], 0, 0);
            return new NegTokenInit(this.mechs, this.mechContext.getFlags(), mechToken, null);
        }


        public virtual bool isEstablished()
        {
            return this.completed && this.mechContext.isEstablished(); 
        }


		/// throws jcifs.spnego.SpnegoException
        private static SpnegoToken getToken(byte[] token, int off, int len)
        {
            byte[] b = new byte[len];
            if (off == 0 && token.Length == len)
            {
                b = token;
            }
            else
            {
                Array.Copy(token, off, b, 0, len);
            }

            return getToken(b);
        }


		/// throws jcifs.spnego.SpnegoException
        private static SpnegoToken getToken(byte[] token)
        {
            SpnegoToken spnegoToken = null;
            try
            {
                switch (token[0])
                {
                    case (byte) 0x60:
                        spnegoToken = new NegTokenInit(token);
                        break;
                    case unchecked((byte) 0xa1):
                        spnegoToken = new NegTokenTarg(token);
                        break;
                    default:
                        throw new SpnegoException("Invalid token type");
                }

                return spnegoToken;
            }
            catch (IOException)
            {
                throw new SpnegoException("Invalid token");
            }
        }


        public virtual bool supportsIntegrity()
        {
            return this.mechContext.supportsIntegrity();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#isPreferredMech(org.bouncycastle.asn1.DerObjectIdentifier) </seealso>
        public virtual bool isPreferredMech(DerObjectIdentifier mech)
        {
            return this.mechContext.isPreferredMech(mech);
        }


		/// throws jcifs.CIFSException
        public virtual byte[] calculateMIC(byte[] data)
        {
            if (!this.completed)
            {
                throw new CIFSException("Context is not established");
            }

            return this.mechContext.calculateMIC(data);
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#verifyMIC(byte[], byte[]) </seealso>
		/// throws jcifs.CIFSException
        public virtual void verifyMIC(byte[] data, byte[] mic)
        {
            if (!this.completed)
            {
                throw new CIFSException("Context is not established");
            }

            this.mechContext.verifyMIC(data, mic);
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.SSPContext#isMICAvailable() </seealso>
        public virtual bool isMICAvailable()
        {
            if (!this.completed)
            {
                return false;
            }

            return this.mechContext.isMICAvailable();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= java.lang.Object#toString() </seealso>
        public override string ToString()
        {
            return "SPNEGO[" + this.mechContext + "]";
        }


        /// 
		/// throws jcifs.CIFSException
        public virtual void dispose()
        {
            this.mechContext.dispose();
        }
    }
}