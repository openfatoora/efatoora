﻿using efatoora.service.Data;
using System.Text;
using System.Xml;
using ZatcaCore;
using ZatcaCore.Contracts;
using ZatcaCore.ESigner;
namespace efatoora.service.Services;

public class InvoiceGenerator(IKeyRepository keyRepository)
{

    public async Task<EInvoiceSignerResponse> GenerateAsync(InvoiceContract invoiceContract)
    {
        Enum.TryParse(invoiceContract.InvoiceType, out InvoiceTypes invoiceType);
        Enum.TryParse(invoiceContract.InvoiceTypeCode, out InvoiceTypeCodes invoiceTypeCode);

        if (invoiceTypeCode == InvoiceTypeCodes.Standard)
        {
            throw new Exception("Invalid Document Type Code");
        }


        var _xmlGenerator = XmlGeneratorFactory.GetXmlGenerator(invoiceType);


        var xmlBytes = _xmlGenerator.Generate(invoiceContract);
        string generatedXML = Encoding.UTF8.GetString(xmlBytes);

        XmlDocument xml = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        IQrGenerator qrGenerator = new QrGenerator();
        xml.LoadXml(generatedXML);
        IHashGenerator hashGenerator = new HashGenerator();
        IDigitalSignartureGenerator digitalSignartureGenerator = new DigitalSignartureGenerator();

        var key = (await keyRepository.GetKeys()).First();

        var eInvoiceSignerResponse = new EInvoiceSigner(hashGenerator, digitalSignartureGenerator, qrGenerator,
            DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"))
            .SignDocument(xml, key.BinaryToken, key.PrivateKey);


        eInvoiceSignerResponse.Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(eInvoiceSignerResponse.Xml));
        return eInvoiceSignerResponse;
    }

}