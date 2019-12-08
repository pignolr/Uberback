using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Uberback.Validator
{
    class AnalyseBatch
    {
        public static string ValidateRequest(Endpoint.AnalyseBatchRequest args)
        {
            if (args == null)
                return "Invalid format of request";
            string error;
            if ((error = ValidateToken(args)) != null
                || (error = ValidateUserId(args)) != null
                || (error = ValidateImages(args)) != null
                || (error = ValidateTexts(args)) != null
                )
                return error;
            return null;

        }

        public static string ValidateImages(Endpoint.AnalyseBatchRequest args)
        {
            if (args.images == null)
                return null;
            try
            {
                foreach (var item in args.images)
                {
                    if (item.urlSrc == null || item.data == null)
                        return "Invalid item in the list of images: doesn't contain \"urlSrc\" or \"data\"";
                    if (!Endpoint.Common.IsAbsoluteUrl(item.urlSrc))
                        return "Invalid item in the list of images: \"urlSrc\" must be an url";
                    // Not a valid way to check if the url is an image
/*                    if (!Endpoint.Common.IsAbsoluteUrl(item.data)
                        && !Endpoint.Common.IsRelativeUrl(item.data))
                        return "Invalid item in the list of images: \"data\" must be an url";
*/                }
            }
            catch
            {
                return "\"images\" must be in json format";
            }
            return null;
        }

        public static string ValidateTexts(Endpoint.AnalyseBatchRequest args)
        {
            if (args.texts ==  null)
                return null;
            try
            {
                foreach (var item in args.texts)
                {
                    if (item.urlSrc == null || item.data == null)
                        return "Invalid item in the list of texts: doesn't contain \"urlSrc\" or \"data\"";
                    if (!Endpoint.Common.IsAbsoluteUrl(item.urlSrc))
                        return "Invalid item in the list of texts: \"urlSrc\" must be an url";
                }
            }
            catch
            {
                return "\"texts\" must be in json format";
            }
            return null;
        }

        public static string ValidateToken(Endpoint.AnalyseBatchRequest args)
        {
            Endpoint.Common.Answer? error = Endpoint.Common.BasicCheck(args.token);
            if (error.HasValue)
                return error.Value.message;
            return null;
        }

        public static string ValidateUserId(Endpoint.AnalyseBatchRequest args)
        {
            if (string.IsNullOrEmpty(args.userId))
                return "The userId is not provided";
            if (!Endpoint.Common.IsUserExists(args.userId))
                return "This userId not exist";
            return null;
        }

    }
}
