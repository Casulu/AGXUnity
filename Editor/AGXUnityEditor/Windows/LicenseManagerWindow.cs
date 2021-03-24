﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AGXUnity.Utils;

using GUI = AGXUnity.Utils.GUI;

namespace AGXUnityEditor.Windows
{
  public class LicenseManagerWindow : EditorWindow
  {
    public static LicenseManagerWindow Open()
    {
      return GetWindow<LicenseManagerWindow>( false,
                                              "License Manager - AGX Dynamics for Unity",
                                              true );
    }

    /// <summary>
    /// License file directory with Assets as root and
    /// / as directory separator.
    /// </summary>
    public string LicenseDirectory
    {
      get
      {
        return GetLicenseDirectoryData().String;
      }
      private set
      {
        if ( Path.IsPathRooted( value ) )
          value = value.MakeRelative( Directory.GetCurrentDirectory(), false ).Replace( '\\', '/' );
        GetLicenseDirectoryData().String = value;
      }
    }

    public bool IsUpdatingLicenseInformation { get { return m_updateLicenseInfoTask != null; } }

    private void OnEnable()
    {
      m_activeLicenseStyle = null;

      ValidateLicenseDirectory();

      StartUpdateLicenseInformation();
    }

    private void OnDisable()
    {
      AGXUnity.LicenseManager.AwaitTasks();
      m_updateLicenseInfoTask?.Wait( 0 );
    }

    private void OnGUI()
    {
      ValidateLicenseDirectory();

      using ( GUI.AlignBlock.Center )
        GUILayout.Box( IconManager.GetAGXUnityLogo(),
                       GUI.Skin.customStyles[ 3 ],
                       GUILayout.Width( 400 ),
                       GUILayout.Height( 100 ) );

      EditorGUILayout.LabelField( "© " + System.DateTime.Now.Year + " Algoryx Simulation AB",
                                  InspectorEditor.Skin.LabelMiddleCenter );

      InspectorGUI.BrandSeparator( 1, 6 );

      m_scroll = EditorGUILayout.BeginScrollView( m_scroll );

      if ( IsUpdatingLicenseInformation )
        ShowNotification( GUI.MakeLabel( "Reading..." ) );
      else if ( AGXUnity.LicenseManager.IsBusy )
        ShowNotification( GUI.MakeLabel( AGXUnity.LicenseManager.IsActivating ?
                                           "Activating..." :
                                           "Refreshing..." ) );

      using ( new GUI.EnabledBlock( !IsUpdatingLicenseInformation && !AGXUnity.LicenseManager.IsBusy ) ) {
        for ( int i = 0; i < m_licenseData.Count; ++i ) {
          var data = m_licenseData[ i ];
          LicenseDataGUI( data );
          if ( i + 1 < m_licenseData.Count )
            InspectorGUI.Separator( 2, 6, InspectorGUISkin.BrandColorBlue );
        }

        if ( m_licenseData.Count > 0 )
          InspectorGUI.Separator( 2, 6, InspectorGUISkin.BrandColorBlue );

        ActivateLicenseGUI();
      }

      EditorGUILayout.EndScrollView();

      if ( AGXUnity.LicenseManager.IsBusy || IsUpdatingLicenseInformation )
        Repaint();
    }

    private void ActivateLicenseGUI()
    {
      GUILayout.Label( GUI.MakeLabel( "Activate license", true ), InspectorEditor.Skin.Label );
      var selectLicenseRect  = GUILayoutUtility.GetLastRect();
      selectLicenseRect.x    += selectLicenseRect.width;
      selectLicenseRect.width = 28;
      selectLicenseRect.x    -= selectLicenseRect.width;
      selectLicenseRect.y    -= EditorGUIUtility.standardVerticalSpacing;
      if ( UnityEngine.GUI.Button( selectLicenseRect,
                                   GUI.MakeLabel( "...",
                                                  InspectorGUISkin.BrandColor,
                                                  true,
                                                  "Select license file on this computer" ),
                                   InspectorEditor.Skin.ButtonMiddle ) ) {
        var sourceLicense = EditorUtility.OpenFilePanel( "Copy AGX Dynamics license file",
                                                         ".",
                                                         $"{AGXUnity.LicenseManager.GetLicenseExtension( AGXUnity.LicenseInfo.LicenseType.Service ).Remove( 0, 1 )}," +
                                                         $"{AGXUnity.LicenseManager.GetLicenseExtension( AGXUnity.LicenseInfo.LicenseType.Legacy ).Remove( 0, 1 )}" );
        if ( !string.IsNullOrEmpty( sourceLicense ) ) {
          var targetLicense = AGXUnity.IO.Environment.FindUniqueFilename( $"{LicenseDirectory}/{Path.GetFileName( sourceLicense )}" ).PrettyPath();
          if ( EditorUtility.DisplayDialog( "Copy AGX Dynamics license",
                                          $"Copy \"{sourceLicense}\" to \"{targetLicense}\"?",
                                          "Yes",
                                          "Cancel" ) ) {
            try {
              File.Copy( sourceLicense, targetLicense, false );
              StartUpdateLicenseInformation();
              GUIUtility.ExitGUI();
            }
            catch ( ExitGUIException ) {
              throw;
            }            
            catch ( System.Exception e ) {
              Debug.LogException( e );
            }
          }
        }
      }

      using ( InspectorGUI.IndentScope.Single ) {
        m_licenseActivateData.Id = EditorGUILayout.TextField( GUI.MakeLabel( "Id" ),
                                                              m_licenseActivateData.Id,
                                                              InspectorEditor.Skin.TextField );
        if ( m_licenseActivateData.Id.Any( c => !char.IsDigit( c ) ) )
          m_licenseActivateData.Id = new string( m_licenseActivateData.Id.Where( c => char.IsDigit( c ) ).ToArray() );
        m_licenseActivateData.Password = EditorGUILayout.PasswordField( GUI.MakeLabel( "Password" ),
                                                                        m_licenseActivateData.Password );

        InspectorGUI.SelectFolder( GUI.MakeLabel( "License File Directory" ),
                                   LicenseDirectory,
                                   "License file directory",
                                   newDirectory =>
                                   {
                                     newDirectory = newDirectory.PrettyPath();

                                     if ( string.IsNullOrEmpty( newDirectory ) )
                                       newDirectory = "Assets";

                                     if ( !Directory.Exists( newDirectory ) ) {
                                       Debug.LogWarning( $"Invalid license directory: {newDirectory} - directory doesn't exist." );
                                       return;
                                     }
                                     LicenseDirectory = newDirectory;
                                   } );

        using ( new GUI.EnabledBlock( UnityEngine.GUI.enabled &&
                                      m_licenseActivateData.Id.Length > 0 &&
                                      m_licenseActivateData.Password.Length > 0 ) ) {
          // It isn't possible to press this button during activation.
          if ( UnityEngine.GUI.Button( EditorGUI.IndentedRect( EditorGUILayout.GetControlRect() ),
                                       GUI.MakeLabel( AGXUnity.LicenseManager.IsBusy ?
                                                        "Activating..." :
                                                        "Activate" ),
                                                      InspectorEditor.Skin.Button ) ) {
            AGXUnity.LicenseManager.ActivateAsync( System.Convert.ToInt32( m_licenseActivateData.Id ),
                                                   m_licenseActivateData.Password,
                                                   LicenseDirectory,
                                                   success =>
                                                   {
                                                     if ( success )
                                                       m_licenseActivateData = IdPassword.Empty();
                                                     StartUpdateLicenseInformation();
                                                   } );
          }
        }
      }
    }

    private void LicenseDataGUI( LicenseData data )
    {
      var highlight = m_licenseData.Count > 1 &&
                      data.LicenseInfo.UniqueId == AGXUnity.LicenseManager.LicenseInfo.UniqueId;
      if ( highlight && m_activeLicenseStyle == null )
        m_activeLicenseStyle = new GUIStyle( InspectorEditor.Skin.Label );
      // The texture is deleted when hitting stop in the editor while
      // m_activeLicenseStyle != null.
      if ( m_activeLicenseStyle != null && m_activeLicenseStyle.normal.background == null )
        m_activeLicenseStyle.normal.background = GUI.CreateColoredTexture( 1,
                                                                           1,
                                                                           Color.Lerp( InspectorGUI.BackgroundColor,
                                                                                       Color.green,
                                                                                       0.025f ) );

      var licenseFileButtons = new InspectorGUI.MiscButtonData[]
      {
        InspectorGUI.MiscButtonData.Create( MiscIcon.Update,
                                            () =>
                                            {
                                              RefreshLicense( data );
                                            },
                                            UnityEngine.GUI.enabled,
                                            "Refresh license from server." ),
        InspectorGUI.MiscButtonData.Create( MiscIcon.EntryRemove,
                                            () =>
                                            {
                                              var deactivateDelete = EditorUtility.DisplayDialog( "Deactivate and erase license.",
                                                                                                  "Would you like to deactivate the current license " +
                                                                                                  "and remove the license file from this project?\n\n" +
                                                                                                  "It's possible to activate the license again in this " +
                                                                                                  "License Manager and/or download the license file again " +
                                                                                                  "from the license portal.",
                                                                                                  "Yes",
                                                                                                  "Cancel" );
                                              if ( deactivateDelete ) {
                                                AGXUnity.LicenseManager.DeactivateAndDelete( data.Filename );
                                                StartUpdateLicenseInformation();
                                                GUIUtility.ExitGUI();
                                              }
                                            },
                                            UnityEngine.GUI.enabled,
                                            "Deactivate and erase license file from project." )
      };

      var highlightScope = highlight ? new EditorGUILayout.VerticalScope( m_activeLicenseStyle ) : null;
      InspectorGUI.SelectableTextField( GUI.MakeLabel( "License file" ),
                                        data.Filename,
                                        licenseFileButtons );
      InspectorGUI.SelectableTextField( GUI.MakeLabel( "License type" ), data.LicenseInfo.TypeDescription );

      InspectorGUI.Separator( 1, 6 );

      InspectorGUI.LicenseEndDateField( data.LicenseInfo );

      EditorGUILayout.EnumFlagsField( GUI.MakeLabel( "Enabled modules",
                                                     false,
                                                     data.LicenseInfo.EnabledModules.ToString() ),
                                      data.LicenseInfo.EnabledModules,
                                      false,
                                      InspectorEditor.Skin.Popup );

      InspectorGUI.SelectableTextField( GUI.MakeLabel( "User" ), data.LicenseInfo.User );

      InspectorGUI.SelectableTextField( GUI.MakeLabel( "Contact" ), data.LicenseInfo.Contact );
      highlightScope?.Dispose();
    }

    private static EditorDataEntry GetLicenseDirectoryData()
    {
      return EditorData.Instance.GetStaticData( "LicenseManagerWindow_LicenseFilename",
                                                entry => entry.String = "Assets" );
    }

    private void ValidateLicenseDirectory()
    {
      if ( !Directory.Exists( LicenseDirectory ) )
        LicenseDirectory = "Assets";
    }

    private void StartUpdateLicenseInformation()
    {
      if ( IsUpdatingLicenseInformation )
        return;

      var currentLicense = AGXUnity.LicenseManager.LicenseInfo.UniqueId;
      var licenseData = new List<LicenseData>();
      m_updateLicenseInfoTask = Task.Run( () =>
      {
        foreach ( var licenseFile in AGXUnity.LicenseManager.FindLicenseFiles() ) {
          AGXUnity.LicenseManager.LoadFile( licenseFile );
          licenseData.Add( new LicenseData()
          {
            Filename = licenseFile,
            LicenseInfo = AGXUnity.LicenseManager.LicenseInfo
          } );
        }

        // Try to load previously loaded license.
        var data = licenseData.Find( d => !string.IsNullOrEmpty( currentLicense ) &&
                                          d.LicenseInfo.UniqueId == currentLicense );
        var successfullyLoadedPrevLicense = data.LicenseInfo.IsParsed &&
                                            data.LicenseInfo.IsValid &&
                                            AGXUnity.LicenseManager.LoadFile( data.Filename );
        // Fall-back to default behavior.
        if ( !successfullyLoadedPrevLicense )
          AGXUnity.LicenseManager.LoadFile();

        return licenseData;
      } );

      EditorApplication.update += OnUpdateLicenseInformation;
    }

    private void OnUpdateLicenseInformation()
    {
      if ( m_updateLicenseInfoTask != null && m_updateLicenseInfoTask.IsCompleted ) {
        m_licenseData = m_updateLicenseInfoTask.Result;
        m_updateLicenseInfoTask = null;
      }

      if ( m_updateLicenseInfoTask == null )
        EditorApplication.update -= OnUpdateLicenseInformation;
    }

    private void RefreshLicense( LicenseData licenseData )
    {
      var prevLicense = m_licenseData.Find( data => data.LicenseInfo.UniqueId == AGXUnity.LicenseManager.LicenseInfo.UniqueId );
      AGXUnity.LicenseManager.RefreshAsync( licenseData.Filename,
                                            success =>
                                            {
                                              UpdateLicenseInfo( licenseData.Filename, AGXUnity.LicenseManager.LicenseInfo );
                                              if ( prevLicense.Filename != licenseData.Filename )
                                                AGXUnity.LicenseManager.LoadFile( prevLicense.Filename );
                                            } );
    }

    private void UpdateLicenseInfo( string filename, AGXUnity.LicenseInfo licenseInfo )
    {
      for ( int i = 0; i < m_licenseData.Count; ++i ) {
        if ( m_licenseData[ i ].Filename == filename )
          m_licenseData[ i ] = new LicenseData() { Filename = filename, LicenseInfo = licenseInfo };
      }
    }

    internal struct IdPassword
    {
      public static IdPassword Empty() { return new IdPassword() { Id = string.Empty, Password = string.Empty }; }
      public string Id;
      public string Password;
    }

    private struct LicenseData
    {
      public string Filename;
      public AGXUnity.LicenseInfo LicenseInfo;
    }

    private IdPassword m_licenseActivateData = IdPassword.Empty();
    private Vector2 m_scroll = Vector2.zero;
    [System.NonSerialized]
    private List<LicenseData> m_licenseData = new List<LicenseData>();
    private Task<List<LicenseData>> m_updateLicenseInfoTask = null;
    [System.NonSerialized]
    private GUIStyle m_activeLicenseStyle = null;
  }
}
