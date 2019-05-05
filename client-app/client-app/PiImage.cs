using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*!
 \file PiImage.cs
 \brief Implementazione classe PiImage e BufferImage
 \version 1.0

 */
namespace client_app
{
    //! \class PiImage
    //! \brief Contiene un immagine(BufferImage) ricevuta dalla Raspberry con la data 
    class PiImage
    {
        //! \var image
        //! \brief Immagine dalla Raspberry
        public BufferImage image;
        //! \var time
        //! \brief Data e ora in cui è stata presa l'immagine in UnixTime
        public long time;
    }
    //! \class BufferImage
    //! \brief Rappresenta un immagine dalla raspberry
    class BufferImage
    {
        //! \var type
        //! \brief tipo dei dati
        public string type;
        //! \var data
        //! \brief Dati ricevuti
        public byte[] data;
    }

}
