using System.Collections;
using System.Reflection;
using NewSafetyHelp.CustomCampaignSystem;
using NewSafetyHelp.CustomCampaignSystem.CustomCampaignModel;
using NewSafetyHelp.CustomDesktop.Utils;
using NewSafetyHelp.Emails;
using NewSafetyHelp.LoggingSystem;
using UnityEngine;

namespace NewSafetyHelp.MainGameBugFixes
{
    public static class EmailLoadingBugFix
    {
        [HarmonyLib.HarmonyPatch(typeof(EmailWindowBehavior), "DisplayEmail", typeof(Email))]
        public static class DisplayEmailPatch
        {
            private static readonly int ScreenLoad = Animator.StringToHash("ScreenLoad");
            
            /// <summary>
            /// DisplayEmail patch to fix the double loading bug.
            /// </summary>
            /// <param name="__instance">Instance of the class.</param>
            /// <param name="emailToDisplay">Email selected to be shown.</param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            private static bool Prefix(EmailWindowBehavior __instance, ref Email emailToDisplay)
            {
                MethodInfo updateLayoutGroupMethod = typeof(EmailWindowBehavior).GetMethod("UpdateLayoutGroup", BindingFlags.NonPublic | BindingFlags.Instance);

                if (updateLayoutGroupMethod == null)
                {
                    LoggingHelper.ErrorLog("Method 'UpdateLayoutGroup' was not found. Calling original function.");
                    return true;
                }
                
                if (__instance.selectedEmail != null)
                {
                    string newEmailBody =
                        $"<b>Subject Line: {__instance.selectedEmail.subjectLine}</b> \n\nFrom: {__instance.selectedEmail.sender}\n\n\n{__instance.selectedEmail.emailBody}";
                    
                    // If we selected the same email, we don't update the email.
                    if (__instance.displayedEmailBody.text.Equals(newEmailBody))
                    {
                        if (__instance.selectedEmail.imageAttachment != null)
                        {
                            if (__instance.displayedImage.sprite.Equals(__instance.selectedEmail.imageAttachment))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    
                    __instance.displayedEmailBody.text = newEmailBody;

                    bool showVideo = false;
                    if (CustomCampaignGlobal.InCustomCampaign)
                    {
                        CustomCampaign customCampaign = CustomCampaignGlobal.GetActiveCustomCampaign();

                        if (customCampaign == null)
                        {
                            LoggingHelper.CampaignNullError();
                            return true;
                        }

                        CustomEmail customEmail = CustomCampaignGlobal.GetCustomEmailFromActiveCampaign(
                            __instance.selectedEmail);

                        LoggingHelper.DebugLog("Trying to find associated custom email. Did we find a custom email? " +
                                               $"{customEmail != null}.");
                    
                        if (customEmail != null)
                        {
                            if (customEmail.HasAnimatedVideo)
                            {
                                LoggingHelper.DebugLog($"Playing email video: '{customEmail.EmailAnimatedVideo}'.");
                                EmailHelper.SetVideoUrlEmail(customEmail.EmailAnimatedVideo);

                                showVideo = true;
                            }
                            else
                            {
                                EmailHelper.RestoreEmailPortrait();
                            }
                        }
                        else
                        {
                            EmailHelper.RestoreEmailPortrait();
                        }
                    }

                    if (showVideo 
                        || __instance.selectedEmail.imageAttachment != null)
                    {
                        __instance.displayedImage.transform.parent.gameObject.SetActive(true);
                    }
                    else
                    {
                        __instance.displayedImage.transform.parent.gameObject.SetActive(false);
                    }

                    if (__instance.selectedEmail.imageAttachment != null)
                    {
                        __instance.displayedImage.sprite = __instance.selectedEmail.imageAttachment;
                    }
                }
                else
                {
                    __instance.displayedEmailBody.text = "No Message to Display.";
                    __instance.displayedImage.transform.parent.gameObject.SetActive(false);
                }
                
                // Fix for double loading issue.
                AnimatorStateInfo state = __instance.loadingHiderAnimator.GetCurrentAnimatorStateInfo(0);

                if (!state.IsName("ScreenLoad")
                    && !__instance.loadingHiderAnimator.IsInTransition(0))
                {
                    __instance.loadingHiderAnimator.SetTrigger(ScreenLoad);
                }
                else
                {
                    __instance.loadingHiderAnimator.ResetTrigger(ScreenLoad);
                    __instance.loadingHiderAnimator.Play(ScreenLoad, 0, 0f);
                }
                
                __instance.previewScrollbar.value = 1f;

                // Original: __instance.UpdateLayoutGroup(__instance.inboxLayoutGroup)
                IEnumerator updateLayoutGroupCoroutine = (IEnumerator) updateLayoutGroupMethod.Invoke(__instance, new object[] {__instance.inboxLayoutGroup}); 
                __instance.StartCoroutine(updateLayoutGroupCoroutine);
                
                return false; // Skip original
            }
        }
    }
}