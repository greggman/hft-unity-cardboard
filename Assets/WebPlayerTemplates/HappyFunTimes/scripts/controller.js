/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
"use strict";

// Start the main app logic.
requirejs([
    'hft/commonui',
    'hft/gameclient',
    'hft/misc/input',
    'hft/misc/misc',
    'hft/misc/mobilehacks',
    'hft/misc/touch',
  ], function(
    CommonUI,
    GameClient,
    Input,
    Misc,
    MobileHacks,
    Touch) {
  var globals = {
    debug: false,
  };
  Misc.applyUrlSettings(globals);
  MobileHacks.fixHeightHack();

  window.hftSettings = window.hftSettings || {};
  window.hftSettings.menu = false;

  var client = new GameClient();

  CommonUI.setupStandardControllerUI(client, globals);

  var camera;
  var scene;
  var renderer;
  var effect;
  var controls;
  var element
  var container;
  var cube;
  var cubeMesh;
  var cubeMaterial;
  var objects = {};
  var meshes = {};

  meshes.cone = new THREE.CylinderGeometry(
    0, 20, 40, 4, 1, false);
  meshes.sphere = new THREE.SphereGeometry(globals.goalSize, 16, 8);
  meshes.box = new THREE.BoxGeometry(10, 10, 10);

  var clock = new THREE.Clock();

  init();
  animate();

  function init() {
    renderer = new THREE.WebGLRenderer();
    element = renderer.domElement;
    container = document.getElementById('example');
    container.appendChild(element);

    effect = new THREE.StereoEffect(renderer);

    scene = new THREE.Scene();

    camera = new THREE.PerspectiveCamera(90, 1, 0.001, 700);
    camera.position.set(0, 10, 0);
    scene.add(camera);

    controls = new THREE.OrbitControls(camera, element);
    controls.rotateUp(Math.PI / 4);
    controls.target.set(
      camera.position.x + 0.1,
      camera.position.y,
      camera.position.z
    );
    controls.noZoom = true;
    controls.noPan = true;

    function sendOrientationToUnity(e) {
      client.sendCmd('orient', {
        alpha: e.alpha,
        beta:  e.beta,
        gamma: e.gamma,
      });
      if (controls.setOrientation) {
        controls.setOrientation(e);
      }
    };

    // These 3 functions show one idea for being
    // able to have Unity tell the phone to
    // add an object to display
    //
    // Unity would send a message like this
    //
    // [CmdName("addObject")]
    // class MessageAddObject {
    //    public string id;
    //    public string mesh;
    // };
    //
    //   MessageAddObject msg = new MessageAddObject();
    //   msg.id = someId;
    //   msg.mesh = "sphere"
    //   m_netPlayer.sendCmd(msg);
    //
    // [CmdName("moveObject")]
    // class MessageMoveObject {
    //    public string id;
    //    public Vector3 pos;
    //    public Vector3 rot;
    // };
    //
    //   MessageMoveObject msg = new MessageMoveObject();
    //   msg.id = someId;
    //   msg.pos = transform.position;
    //   msg.rot = transform.eulerAngles;
    //   m_netPlayer.sendCmd(msg);
    //
    // [CmdName("removeObject")]
    // class MessageRemoveObject {
    //    public string id;
    // };
    //
    //   MessageRemoveObject msg = new MessageRemoveObject();
    //   msg.id = someId;
    //   m_netPlayer.sendCmd(msg);

    // The issue with this is if a player joins late
    // you must send all objects to them.


    //function addObject(msg) {
    //  var material = new THREE.MeshPhongMaterial({
    //    ambient: 0x808080,
    //    color: 0x8080FF,
    //    specular: 0xFFFFFF,
    //    shininess: 30,
    //    shading: THREE.FlatShading,
    //  });
    //  var mesh = new THREE.Mesh(meshes[msg.mesh], material);
    //  scene.add(mesh);
    //  var obj = {
    //    mesh: mesh,
    //    material: material,
    //  };
    //  objects[id] = obj;
    //};
    //
    //function moveObject(msg) {
    //  var obj = objects[msg.id];
    //  if (obj) {
    //    obj.mesh.position.x = msg.pos.x;
    //    obj.mesh.position.y = msg.pos.y;
    //    obj.mesh.position.z = msg.pos.z;
    //    obj.mesh.rotation.x = msg.rot.x;
    //    obj.mesh.rotation.y = msg.rot.y;
    //    obj.mesh.rotation.z = msg.rot.z;
    //  }
    //};
    //
    //function removeObject(msg) {
    //  var obj = objects[msg.id];
    //  if (obj) {
    //    scene.remove(obj.mesh);
    //    delete obj[msg.id];
    //  }
    //};
    //
    //client.addEventListener('addObject',    addObject);
    //client.addEventListener('moveObject',   moveObject);
    //client.addEventListener('removeObject', removeObject);

    function setOrientationControls(e) {
      if (!e.alpha) {
        return;
      }

      controls = new THREE.DeviceOrientationControls(camera, true);
      controls.connect();
      controls.update();

      element.addEventListener('click', fullscreen, false);

      window.removeEventListener('deviceorientation', setOrientationControls);
      window.addEventListener('deviceorientation', sendOrientationToUnity, false);
    }
    window.addEventListener('deviceorientation', setOrientationControls, false);


    var light = new THREE.HemisphereLight(0x777777, 0x000000, 0.6);
    scene.add(light);

    var texture = THREE.ImageUtils.loadTexture(
      'textures/patterns/checker.png'
    );
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    texture.repeat = new THREE.Vector2(50, 50);
    texture.anisotropy = renderer.getMaxAnisotropy();

    var material = new THREE.MeshPhongMaterial({
      color: 0xffffff,
      specular: 0xffffff,
      shininess: 20,
      shading: THREE.FlatShading,
      map: texture
    });

    var geometry = new THREE.PlaneGeometry(1000, 1000);

    var mesh = new THREE.Mesh(geometry, material);
    mesh.rotation.x = -Math.PI / 2;
    scene.add(mesh);

    cubeMaterial = new THREE.MeshPhongMaterial({
      color: 0xff8040,
      specular: 0xffffff,
      shininess: 20,
      shading: THREE.FlatShading,
    });

    cube = new THREE.BoxGeometry(5, 5, 5);
    cubeMesh = new THREE.Mesh(cube, cubeMaterial);
    cubeMesh.position.x = 10;
    cubeMesh.position.y = 7;
    scene.add(cubeMesh);

    window.addEventListener('resize', resize, false);
    setTimeout(resize, 1);
  }

  function resize() {
    var width = container.clientWidth;
    var height = container.clientHeight;

    camera.aspect = width / height;
    camera.updateProjectionMatrix();

    renderer.setSize(width, height);
    effect.setSize(width, height);
  }

  function update(dt) {
    resize();

    camera.updateProjectionMatrix();

    cubeMesh.rotation.y += dt;

    controls.update(dt);
  }

  function render(dt) {
    effect.render(scene, camera);
  }

  function animate(t) {
    update(clock.getDelta());
    render(clock.getDelta());
    requestAnimationFrame(animate);

  }

  function fullscreen() {
    if (container.requestFullscreen) {
      container.requestFullscreen();
    } else if (container.msRequestFullscreen) {
      container.msRequestFullscreen();
    } else if (container.mozRequestFullScreen) {
      container.mozRequestFullScreen();
    } else if (container.webkitRequestFullscreen) {
      container.webkitRequestFullscreen();
    }
  }
});

