﻿using com.csutil.model;
using com.csutil.model.jsonschema;
using com.csutil.ui;
using com.csutil.ui.jsonschema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests.jsonschema {

    public class Ui18_1_JsonSchemaUiGenerator : UnitTestMono {

        public override IEnumerator RunTest() {
            yield return GenerateAndShowViewFor(gameObject.GetViewStack()).AsCoroutine();
        }

        private static async Task GenerateAndShowViewFor(ViewStack viewStack) {
            {
                await Dialog.ShowInfoDialog("Generating UIs from C# classes", "The next examples will show generating" +
                    " views from normal C# classes. In this example MyUserModel is passed to the generator", "Show example");
                var schemaGenerator = new ModelToJsonSchema();
                var userModelType = typeof(MyUserModel);
                JsonSchema schema = schemaGenerator.ToJsonSchema("" + userModelType, userModelType);
                Log.d(JsonWriter.AsPrettyString(schema));
                await LoadModelIntoGeneratedView(viewStack, schemaGenerator, schema);
            }
            {
                await Dialog.ShowInfoDialog("Generating UIs from JSON schemas",
                    "This time the json schema is loaded from an JSON schema string", "Show example");
                var schemaGenerator = new ModelToJsonSchema();
                JsonSchema schema = JsonReader.GetReader().Read<JsonSchema>(SomeJsonSchemaExamples.jsonSchema1);
                await LoadJsonModelIntoGeneratedJsonSchemaView(viewStack, schemaGenerator, schema, SomeJsonSchemaExamples.json1);
            }
            {
                await Dialog.ShowInfoDialog("Editing arrays & lists:",
                    "Both primitave lists but also List<MyClass1> can be shown and edited", "Show list example");
                var schemaGenerator = new ModelToJsonSchema();
                JsonSchema schema = JsonReader.GetReader().Read<JsonSchema>(SomeJsonSchemaExamples.jsonSchema2);
                await LoadJsonModelIntoGeneratedJsonSchemaView(viewStack, schemaGenerator, schema, SomeJsonSchemaExamples.json2);
            }
        }

        private static async Task LoadModelIntoGeneratedView(ViewStack viewStack, ModelToJsonSchema schemaGenerator, JsonSchema schema) {
            MyUserModel model = NewExampleUserInstance();
            {
                await Dialog.ShowInfoDialog("Manually connecting the model instance to the view", "First an example to connect the " +
                    "model to a generated view via a manual presenter 'MyManualPresenter1'", "Show manual presenter example");
                var viewGenerator = new JsonSchemaToView(schemaGenerator);
                GameObject generatedView = await viewGenerator.ToView(schema);
                viewStack.ShowView(generatedView);

                var presenter = new MyManualPresenter1();
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                await presenter.LoadModelIntoView(model);
                viewStack.SwitchBackToLastView(generatedView);
                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(model));
            }
            {
                await Dialog.ShowInfoDialog("Using JsonSchemaPresenter to autmatically connect the model instance and view",
                    "The second option is to use a generic JObjectPresenter to connect the model to the generated view",
                    "Show JsonSchemaPresenter example");
                var viewGenerator = new JsonSchemaToView(schemaGenerator);
                GameObject generatedView = await viewGenerator.ToView(schema);
                viewStack.ShowView(generatedView);

                var presenter = new JsonSchemaPresenter(viewGenerator);
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                MyUserModel changedModel = await presenter.LoadViaJsonIntoView(model);
                viewStack.SwitchBackToLastView(generatedView);

                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(changedModel));
                var changedFields = MergeJson.GetDiff(model, changedModel);
                Log.d("Fields changed: " + changedFields?.ToPrettyString());
            }
        }

        private static async Task LoadJsonModelIntoGeneratedJsonSchemaView(
                                ViewStack viewStack, ModelToJsonSchema schemaGenerator, JsonSchema schema, string jsonModel) {

            JObject model = JsonReader.GetReader().Read<JObject>(jsonModel);
            JsonSchemaToView viewGenerator = new JsonSchemaToView(schemaGenerator);
            GameObject generatedView = await viewGenerator.ToView(schema);

            viewStack.ShowView(generatedView);

            var presenter = new JsonSchemaPresenter(viewGenerator);
            presenter.targetView = generatedView;

            Log.d("Model BEFORE changes: " + model.ToPrettyString());
            var changedModel = await presenter.LoadModelIntoView(model.DeepClone() as JObject);
            await JsonSchemaPresenter.ChangesSavedViaConfirmButton(generatedView);
            Log.d("Model AFTER changes: " + changedModel.ToPrettyString());

            viewStack.SwitchBackToLastView(generatedView);
            var changedFields = MergeJson.GetDiff(model, changedModel);
            Log.d("Fields changed: " + changedFields?.ToPrettyString());

        }

        private class MyManualPresenter1 : Presenter<MyUserModel> {

            public GameObject targetView { get; set; }

            public async Task OnLoad(MyUserModel u) {
                var map = targetView.GetFieldViewMap();
                map.LinkViewToModel("id", u.id);
                map.LinkViewToModel("name", u.name, newVal => u.name = newVal);
                map.LinkViewToModel("lastName", u.lastName, newVal => u.lastName = newVal);
                map.LinkViewToModel("email", u.email, newVal => u.email = newVal);
                map.LinkViewToModel("password", u.password, newVal => u.password = newVal);
                map.LinkViewToModel("age", "" + u.age, newVal => u.age = int.Parse(newVal));
                map.LinkViewToModel("money", "" + u.money, newVal => u.money = float.Parse(newVal));
                map.LinkViewToModel("phoneNumber", u.phoneNumber, newVal => u.phoneNumber = newVal);
                map.LinkViewToModel("description", u.description, newVal => u.description = newVal);
                map.LinkViewToModel("homepage", u.homepage, newVal => u.homepage = newVal);
                map.LinkViewToModel("hasMoney", u.hasMoney, newVal => u.hasMoney = newVal);
                map.LinkViewToModel("exp1", u.exp1, newVal => u.exp1 = newVal);
                map.LinkViewToModel("exp2", "" + u.exp2, newVal => u.exp2 = MyUserModel.Experience.Avg.TryParse(newVal));

                map.LinkViewToModel("bestFriend.name", u.bestFriend.name, newVal => u.bestFriend.name = newVal);
                map.LinkViewToModel("profilePic.url", u.profilePic.url, newVal => u.profilePic.url = newVal);

                await JsonSchemaPresenter.ChangesSavedViaConfirmButton(targetView);
            }

        }

        internal static MyUserModel NewExampleUserInstance() {
            return new MyUserModel() {
                name = "Tom",
                email = "a@b.com",
                password = "12345678",
                age = 98,
                money = 0f,
                hasMoney = false,
                phoneNumber = "+1 234 5678 90",
                profilePic = new MyUserModel.FileRef() { url = "https://picsum.photos/50/50" },
                bestFriend = new MyUserModel.UserContact() {
                    name = "Bella",
                    user = new MyUserModel() {
                        name = "Bella",
                        email = "b@cd.e"
                    }
                },
                description = "A normal person, nothing suspicious here..",
                homepage = "https://marvolo.uk"
            };
        }

        internal class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; private set; } = Guid.NewGuid().ToString();

            [System.ComponentModel.DataAnnotations.Range(2, 30)]
            [Content(ContentFormat.name, "e.g. Tom")]
            public string name;

            [System.ComponentModel.DataAnnotations.Required]
            [Content(ContentFormat.name, "e.g. Riddle")]
            public string lastName;

            [Content(ContentFormat.email, "e.g. tom@email.com")]
            [Regex(RegexTemplates.EMAIL_ADDRESS)]
            public string email;

            [Content(ContentFormat.password, "Lenght >= 6 & has A-Z a-z 0-9 ?!..")]
            [System.ComponentModel.DataAnnotations.Range(minimum: 6, maximum: 100)]
            [Regex(RegexTemplates.HAS_UPPERCASE, RegexTemplates.HAS_LOWERCASE, RegexTemplates.HAS_NUMBER, RegexTemplates.HAS_SPECIAL_CHAR)]
            public string password;

            [System.ComponentModel.Description("e.g. 99")]
            [System.ComponentModel.DataAnnotations.Range(0, 130)]
            public int? age;

            [System.ComponentModel.Description("e.g. 99")]
            [System.ComponentModel.DataAnnotations.Range(0, 160)]
            public int? progress { get; private set; } = 60;

            public float money;

            public enum Experience { Beginner, Avg, Expert }
            [Enum("Level of experience 1", typeof(Experience), allowOtherInput = true)]
            public string exp1;

            [System.ComponentModel.Description("Level of experience 2")]
            public Experience exp2;

            [System.ComponentModel.Description("Checked if there is any money")]
            public bool hasMoney;

            [Regex(RegexTemplates.PHONE_NR)]
            [System.ComponentModel.Description("e.g. +1 234 5678 90")]
            public string phoneNumber;

            public FileRef profilePic;

            public UserContact bestFriend;

            [Content(ContentFormat.essay, "A detailed description")]
            public string description;

            [Regex(RegexTemplates.URL)]
            public string homepage;

            //public List<string> tags { get; set; }

            //public List<UserContact> contacts { get; } = new List<UserContact>();

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
            public class UserContact {

                [Content(ContentFormat.name, "e.g. Barbara")]
                public string name;
                public MyUserModel user;

                //public int[] phoneNumbers { get; set; }

            }

            internal class FileRef : IFileRef {

                [Regex(RegexTemplates.URL)]
                public string url { get; set; }

                public string dir { get; set; }
                public string fileName { get; set; }
                public Dictionary<string, object> checksums { get; set; }
                public string mimeType { get; set; }

            }

        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}
