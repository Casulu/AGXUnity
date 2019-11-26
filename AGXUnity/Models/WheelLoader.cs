﻿using System.Collections.Generic;
using UnityEngine;

namespace AGXUnity.Models
{
  public class WheelLoader : ScriptComponent
  {
    public enum DifferentialLocation
    {
      Rear,
      Center,
      Front
    }

    public enum WheelLocation
    {
      LeftFront,
      RightFront,
      LeftRear,
      RightRear
    }

    [SerializeField]
    private bool m_engineEnabled = true;

    [InspectorGroupBegin( Name = "Engine" )]
    public bool EngineEnabled
    {
      get { return m_engineEnabled; }
      set
      {
        m_engineEnabled = value;
        if ( Engine != null )
          Engine.setEnable( m_engineEnabled );
      }
    }

    [SerializeField]
    private float m_inletVolume = 0.015f;

    [ClampAboveZeroInInspector]
    public float InletVolume
    {
      get { return m_inletVolume; }
      set
      {
        m_inletVolume = value;
        if ( Engine != null )
          Engine.setInletVolume( m_inletVolume );
      }
    }

    [SerializeField]
    private float m_volumetricEfficiency = 0.9f;

    [ClampAboveZeroInInspector]
    public float VolumetricEfficiency
    {
      get { return m_volumetricEfficiency; }
      set
      {
        m_volumetricEfficiency = value;
        if ( Engine != null )
          Engine.setVolumetricEfficiency( m_volumetricEfficiency );
      }
    }

    [SerializeField]
    private float m_throttle = 0.0f;

    [FloatSliderInInspector( 0.0f, 1.0f )]
    public float Throttle
    {
      get { return m_throttle; }
      set
      {
        m_throttle = value;
        if ( Engine != null )
          Engine.setThrottle( m_throttle );
      }
    }

    [SerializeField]
    private float m_idleThrottleAngle = 0.17f;

    [ClampAboveZeroInInspector( true )]
    public float IdleThrottleAngle
    {
      get { return m_idleThrottleAngle; }
      set
      {
        m_idleThrottleAngle = Mathf.Min( value, 1.45f );
        if ( Engine != null )
          Engine.setIdleThrottleAngle( m_idleThrottleAngle );
      }
    }

    [SerializeField]
    private float m_throttleBore = 0.3062f;

    [ClampAboveZeroInInspector]
    public float ThrottleBore
    {
      get { return m_throttleBore; }
      set
      {
        m_throttleBore = value;
        if ( Engine != null )
          Engine.setThrottleBore( m_throttleBore );
      }
    }

    [SerializeField]
    private float m_dischargeCoefficient = 0.7f;

    [ClampAboveZeroInInspector( true )]
    public float DischargeCoefficient
    {
      get { return m_dischargeCoefficient; }
      set
      {
        m_dischargeCoefficient = Mathf.Min( value, 1.0f );
        if ( Engine != null )
          Engine.setDischargeCoefficient( m_dischargeCoefficient );
      }
    }

    [SerializeField]
    private float m_numberOfRevolutionsPerCycle = 2.0f;

    [ClampAboveZeroInInspector]
    public float NumberOfRevolutionsPerCycle
    {
      get { return m_numberOfRevolutionsPerCycle; }
      set
      {
        m_numberOfRevolutionsPerCycle = Mathf.Max( value, 1.0f );
        if ( Engine != null )
          Engine.setNrRevolutionsPerCycle( m_numberOfRevolutionsPerCycle );
      }
    }

    [SerializeField]
    private Vector2 m_gearRatios = new Vector2( -5.0f, 5.0f );

    [InspectorGroupBegin(Name = "Gear Box")]
    public Vector2 GearRatios
    {
      get { return m_gearRatios; }
      set
      {
        m_gearRatios = value;
        if ( GearBox != null )
          GearBox.setGearRatios( new agx.RealVector( new double[] { m_gearRatios.x, m_gearRatios.y } ) );
      }
    }

    [SerializeField]
    private float m_rearDifferentialGearRatio = 1.0f;

    [InspectorGroupBegin( Name = "Differentials" )]
    public float RearDifferentialGearRatio
    {
      get { return m_rearDifferentialGearRatio; }
      set
      {
        m_rearDifferentialGearRatio = value;
        if ( Differentials[ (int)DifferentialLocation.Rear ] != null )
          Differentials[ (int)DifferentialLocation.Rear ].setGearRatio( m_rearDifferentialGearRatio );
      }
    }

    [SerializeField]
    private bool m_rearDifferentialLocked = false;

    public bool RearDifferentialLocked
    {
      get { return m_rearDifferentialLocked; }
      set
      {
        m_rearDifferentialLocked = value;
        if ( Differentials[ (int)DifferentialLocation.Rear ] != null )
          Differentials[ (int)DifferentialLocation.Rear ].setLock( m_rearDifferentialLocked );
      }
    }

    [SerializeField]
    private float m_centerDifferentialGearRatio = 10.0f;

    public float CenterDifferentialGearRatio
    {
      get { return m_centerDifferentialGearRatio; }
      set
      {
        m_centerDifferentialGearRatio = value;
        if ( Differentials[ (int)DifferentialLocation.Center ] != null )
          Differentials[ (int)DifferentialLocation.Center ].setGearRatio( m_centerDifferentialGearRatio );
      }
    }

    [SerializeField]
    private bool m_centerDifferentialLocked = true;

    public bool CenterDifferentialLocked
    {
      get { return m_centerDifferentialLocked; }
      set
      {
        m_centerDifferentialLocked = value;
        if ( Differentials[ (int)DifferentialLocation.Center ] != null )
          Differentials[ (int)DifferentialLocation.Center ].setLock( m_centerDifferentialLocked );
      }
    }

    [SerializeField]
    private float m_frontDifferentialGearRatio = 1.0f;

    public float FrontDifferentialGearRatio
    {
      get { return m_frontDifferentialGearRatio; }
      set
      {
        m_frontDifferentialGearRatio = value;
        if ( Differentials[ (int)DifferentialLocation.Front ] != null )
          Differentials[ (int)DifferentialLocation.Front ].setGearRatio( m_frontDifferentialGearRatio );
      }
    }

    [SerializeField]
    private bool m_frontDifferentialLocked = false;

    public bool FrontDifferentialLocked
    {
      get { return m_frontDifferentialLocked; }
      set
      {
        m_frontDifferentialLocked = value;
        if ( Differentials[ (int)DifferentialLocation.Front ] != null )
          Differentials[ (int)DifferentialLocation.Front ].setLock( m_frontDifferentialLocked );
      }
    }

    [InspectorGroupBegin( Name = "Wheels" )]
    [AllowRecursiveEditing]
    public RigidBody LeftFrontWheel { get { return GetOrFindWheel( WheelLocation.LeftFront ); } }
    [AllowRecursiveEditing]
    public RigidBody RightFrontWheel { get { return GetOrFindWheel( WheelLocation.RightFront ); } }
    [AllowRecursiveEditing]
    public RigidBody LeftRearWheel { get { return GetOrFindWheel( WheelLocation.LeftRear ); } }
    [AllowRecursiveEditing]
    public RigidBody RightRearWheel { get { return GetOrFindWheel( WheelLocation.RightRear ); } }

    [InspectorGroupBegin( Name = "Wheel Hinges" )]
    [AllowRecursiveEditing]
    public Constraint RightRearHinge { get { return GetOrFindConstraint( WheelLocation.RightRear, "Hinge", m_wheelHinges ); } }
    [AllowRecursiveEditing]
    public Constraint LeftRearHinge { get { return GetOrFindConstraint( WheelLocation.LeftRear, "Hinge", m_wheelHinges ); } }
    [AllowRecursiveEditing]
    public Constraint RightFrontHinge { get { return GetOrFindConstraint( WheelLocation.RightFront, "Hinge", m_wheelHinges ); } }
    [AllowRecursiveEditing]
    public Constraint LeftFrontHinge { get { return GetOrFindConstraint( WheelLocation.LeftFront, "Hinge", m_wheelHinges ); } }

    [InspectorGroupBegin( Name = "Controlled Constraints")]
    [AllowRecursiveEditing]
    public Constraint SteeringHinge
    {
      get
      {
        if ( m_steeringHinge == null )
          m_steeringHinge = transform.Find( "WaistHinge" ).GetComponent<Constraint>();
        return m_steeringHinge;
      }
    }

    [AllowRecursiveEditing]
    public Constraint LeftElevatePrismatic
    {
      get
      {
        if ( m_elevatePrismatics[ 0 ] == null )
          m_elevatePrismatics[ 0 ] = FindChild<Constraint>( "LeftLowerPrismatic" );
        return m_elevatePrismatics[ 0 ];
      }
    }

    [AllowRecursiveEditing]
    public Constraint RightElevatePrismatic
    {
      get
      {
        if ( m_elevatePrismatics[ 1 ] == null )
          m_elevatePrismatics[ 1 ] = FindChild<Constraint>( "RightLowerPrismatic" );
        return m_elevatePrismatics[ 1 ];
      }
    }

    [AllowRecursiveEditing]
    public Constraint TiltPrismatic
    {
      get
      {
        if ( m_tiltPrismatic == null )
          m_tiltPrismatic = FindChild<Constraint>( "CenterPrismatic" );
        return m_tiltPrismatic;
      }
    }

    [InspectorGroupEnd]
    [HideInInspector]
    public agx.Hinge BrakeHinge { get; private set; } = null;
    public agxPowerLine.PowerLine PowerLine { get; private set; } = null;
    public agxDriveTrain.CombustionEngine Engine { get; private set; } = null;
    public agxDriveTrain.GearBox GearBox { get; private set; } = null;
    public agxDriveTrain.Differential[] Differentials { get; private set; } = new agxDriveTrain.Differential[] { null, null, null };
    public agxDriveTrain.TorqueConverter TorqueConverter { get; private set; } = null;

    public IEnumerable<Constraint> WheelHinges
    {
      get
      {
        yield return LeftFrontHinge;
        yield return RightFrontHinge;
        yield return LeftRearHinge;
        yield return RightRearHinge;
      }
    }

    protected override bool Initialize()
    {
      PowerLine = new agxPowerLine.PowerLine();
      PowerLine.setName( name );
      Engine = new agxDriveTrain.CombustionEngine( InletVolume );
      TorqueConverter = new agxDriveTrain.TorqueConverter();
      GearBox = new agxDriveTrain.GearBox();
      Differentials[ (int)DifferentialLocation.Rear ]   = new agxDriveTrain.Differential();
      Differentials[ (int)DifferentialLocation.Center ] = new agxDriveTrain.Differential();
      Differentials[ (int)DifferentialLocation.Front ]  = new agxDriveTrain.Differential();

      m_actuators[ (int)WheelLocation.LeftFront ]  = new agxPowerLine.RotationalActuator( LeftFrontHinge.GetInitialized<Constraint>().Native.asHinge() );
      m_actuators[ (int)WheelLocation.RightFront ] = new agxPowerLine.RotationalActuator( RightFrontHinge.GetInitialized<Constraint>().Native.asHinge() );
      m_actuators[ (int)WheelLocation.LeftRear ]   = new agxPowerLine.RotationalActuator( LeftRearHinge.GetInitialized<Constraint>().Native.asHinge() );
      m_actuators[ (int)WheelLocation.RightRear ]  = new agxPowerLine.RotationalActuator( RightRearHinge.GetInitialized<Constraint>().Native.asHinge() );

      foreach ( var wheelHinge in WheelHinges )
        wheelHinge.GetController<TargetSpeedController>().Enable = false;

      var engineTorqueConverterShaft    = new agxDriveTrain.Shaft();
      var torqueConverterGearBoxShaft   = new agxDriveTrain.Shaft();
      var gearBoxCenterDiffShaft        = new agxDriveTrain.Shaft();
      var centerDiffRearDiffShaft       = new agxDriveTrain.Shaft();
      var centerDiffFrontDiffShaft      = new agxDriveTrain.Shaft();
      var frontDiffFrontLeftWheelShaft  = new agxDriveTrain.Shaft();
      var frontDiffFrontRightWheelShaft = new agxDriveTrain.Shaft();
      var rearDiffRearLeftWheelShaft    = new agxDriveTrain.Shaft();
      var rearDiffRearRightWheelShaft   = new agxDriveTrain.Shaft();

      PowerLine.setSource( Engine );

      var INPUT  = agxPowerLine.UnitSide.UNIT_INPUT;
      var OUTPUT = agxPowerLine.UnitSide.UNIT_OUTPUT;

      Engine.connect( OUTPUT, INPUT, engineTorqueConverterShaft );
      engineTorqueConverterShaft.connect( TorqueConverter );
      TorqueConverter.connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, torqueConverterGearBoxShaft );
      torqueConverterGearBoxShaft.connect( GearBox );
      GearBox.connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, gearBoxCenterDiffShaft );
      gearBoxCenterDiffShaft.connect( Differentials[ (int)DifferentialLocation.Center ] );

      Differentials[ (int)DifferentialLocation.Center ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, centerDiffFrontDiffShaft );
      centerDiffFrontDiffShaft.connect( Differentials[ (int)DifferentialLocation.Front ] );
      Differentials[ (int)DifferentialLocation.Front ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, frontDiffFrontLeftWheelShaft );
      Differentials[ (int)DifferentialLocation.Front ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, frontDiffFrontRightWheelShaft );
      frontDiffFrontLeftWheelShaft.connect( m_actuators[ (int)WheelLocation.LeftFront ] );
      frontDiffFrontRightWheelShaft.connect( m_actuators[ (int)WheelLocation.RightFront ] );

      Differentials[ (int)DifferentialLocation.Center ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, centerDiffRearDiffShaft );
      centerDiffRearDiffShaft.connect( Differentials[ (int)DifferentialLocation.Rear ] );
      Differentials[ (int)DifferentialLocation.Rear ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, rearDiffRearLeftWheelShaft );
      Differentials[ (int)DifferentialLocation.Rear ].connect( (agxPowerLine.ConnectorSide)OUTPUT, INPUT, rearDiffRearRightWheelShaft );
      rearDiffRearLeftWheelShaft.connect( m_actuators[ (int)WheelLocation.LeftRear ] );
      rearDiffRearRightWheelShaft.connect( m_actuators[ (int)WheelLocation.RightRear ] );

      var munu = new agx.RealPairVector( new agx.RealPair[]
      {
        new agx.RealPair( -0.0001, 0.00 ),
        new agx.RealPair( 0.00001, 0.50 ),
        new agx.RealPair( 0.00011, 2.00 ),
        new agx.RealPair( 0.00100, 2.00 ),
        new agx.RealPair( 0.20000, 1.10 ),
        new agx.RealPair( 0.40000, 1.15 ),
        new agx.RealPair( 0.60000, 1.05 ),
        new agx.RealPair( 0.80000, 1.01 ),
        new agx.RealPair( 0.90000, 0.99 ),
        new agx.RealPair( 1.00000, 0.98 ),
        new agx.RealPair( 1.00100, 0.98 )
      } );
      TorqueConverter.setMuTable( munu );
      TorqueConverter.setMaxMultiplication( 2.0 );
      TorqueConverter.setPumpTorqueReferenceRPM( 1000.0 );

      GearBox.setGearRatios( new agx.RealVector( new double[] { GearRatios.x, GearRatios.y } ) );
      GearBox.gearUp();

      GetSimulation().add( PowerLine );

      var f1 = new agx.Frame();
      var f2 = new agx.Frame();
      agx.Constraint.calculateFramesFromBody( new agx.Vec3(),
                                              agx.Vec3.X_AXIS(),
                                              gearBoxCenterDiffShaft.getRotationalDimension().getOrReserveBody(),
                                              f1,
                                              null,
                                              f2 );
      BrakeHinge = new agx.Hinge( gearBoxCenterDiffShaft.getRotationalDimension().getOrReserveBody(),
                                  f1,
                                  null,
                                  f2 );
      GetSimulation().add( BrakeHinge );

      return true;
    }

    protected override void OnDestroy()
    {
      if ( GetSimulation() != null ) {
        GetSimulation().remove( PowerLine );
        GetSimulation().remove( BrakeHinge );
      }

      PowerLine       = null;
      BrakeHinge      = null;
      Engine          = null;
      GearBox         = null;
      TorqueConverter = null;
      Differentials[ (int)DifferentialLocation.Rear ]   = null;
      Differentials[ (int)DifferentialLocation.Center ] = null;
      Differentials[ (int)DifferentialLocation.Front ]  = null;
      m_actuators[ (int)WheelLocation.RightRear ]  = null;
      m_actuators[ (int)WheelLocation.LeftRear ]   = null;
      m_actuators[ (int)WheelLocation.RightFront ] = null;
      m_actuators[ (int)WheelLocation.LeftFront ]  = null;

      base.OnDestroy();
    }

    private T FindChild<T>( string name )
      where T : ScriptComponent
    {
      return transform.Find( name ).GetComponent<T>();
    }

    private RigidBody GetOrFindWheel( WheelLocation location )
    {
      if ( m_wheelBodies[ (int)location ] == null )
        m_wheelBodies[ (int)location ] = FindChild<RigidBody>( location.ToString() + "Tire" );
      return m_wheelBodies[ (int)location ];
    }

    private Constraint GetOrFindConstraint( WheelLocation location, string postfix, Constraint[] cache )
    {
      if ( cache[ (int)location ] == null )
        cache[ (int)location ] = FindChild<Constraint>( location.ToString() + postfix );
      return cache[ (int)location ];
    }

    private agxPowerLine.RotationalActuator[] m_actuators = new agxPowerLine.RotationalActuator[] { null, null, null, null };

    private RigidBody[] m_wheelBodies = new RigidBody[] { null, null, null, null };
    private Constraint[] m_wheelHinges = new Constraint[] { null, null, null, null };
    private Constraint m_steeringHinge = null;

    private Constraint[] m_elevatePrismatics = new Constraint[] { null, null };
    private Constraint m_tiltPrismatic = null;
  }
}