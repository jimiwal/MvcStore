using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Domain.Abstract;
using Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebUI.Controllers;
using WebUI.Helpers;
using WebUI.Models;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ ProductID = 1, Name="Name1" },
                new Product{ ProductID = 2, Name="Name2" },
                new Product{ ProductID = 3, Name="Name3" },
                new Product{ ProductID = 4, Name="Name4" },
                new Product{ ProductID = 5, Name="Name5" }
            }.AsQueryable());

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Action
            Product[] prodArray = ((ProductListViewModel)controller.List(null, 2).Model).Products.ToArray();

            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "Name4");
            Assert.AreEqual(prodArray[1].Name, "Name5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            //Arrange
            HtmlHelper myHelper = null;

            PagingInfo pagingInfo = new PagingInfo()
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            Func<int, String> pageUrlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual(result.ToString(), @"<a href=""Page1"">1</a><a class=""selected"" href=""Page2"">2</a><a href=""Page3"">3</a>");

        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "AB1"},
                new Product {ProductID = 2, Name = "AB2"},
                new Product {ProductID = 3, Name = "AB3"},
                new Product {ProductID = 4, Name = "AB4"},
                new Product {ProductID = 5, Name = "AB5"},
            }.AsQueryable());

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Action
            ProductListViewModel result = (ProductListViewModel)controller.List(null, 2).Model;

            //Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
        }

        [TestMethod]
        public void Can_Filter_Products()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns( new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "A"},
                new Product {ProductID = 2, Name = "P2", Category = "B"},
                new Product {ProductID = 3, Name = "P3", Category = "A"},
                new Product {ProductID = 4, Name = "P4", Category = "B"},
                new Product {ProductID = 5, Name = "P5", Category = "C"},
            }.AsQueryable());

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Action
            Product[] result = ((ProductListViewModel)controller.List("B", 1).Model).Products.ToArray();

            //Assert
            Assert.AreEqual(result.Length, 2);
            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "B");
            Assert.IsTrue(result[1].Name == "P4" && result[1].Category == "B");

        }

        [TestMethod]
        private void Can_Create_Categories()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product { ProductID = 1, Name="P1", Category="A" },
                new Product { ProductID = 2, Name="P2", Category="A" },
                new Product { ProductID = 3, Name="P3", Category="B" },
                new Product { ProductID = 4, Name="P4", Category="C" },
                new Product { ProductID = 5, Name="P5", Category="B" },
            }.AsQueryable());

            //Arrange
            NavController target = new NavController(mock.Object);

            //Act
            string[] result = ((IEnumerable<string>)target.Menu().Model).ToArray();

            //Assert
            Assert.AreEqual(result.Length, 3);
            Assert.AreEqual(result[0], "A");
            Assert.AreEqual(result[0], "B");
            Assert.AreEqual(result[0], "C");
        }

        [TestMethod]
        private void Indicates_Selected_Category()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name="Name1", Category = "Cat1"},
                new Product {ProductID = 2, Name="Name2", Category = "Cat2"},
            }.AsQueryable());

            //Arrange - create the controller
            NavController target = new NavController(mock.Object);

            //Arrange - definte the category selected
            string categoryToSelect = "Cat1";

            //Action
            string result = (string)target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            //Assert
            Assert.AreEqual(categoryToSelect, result);
        }

        [TestMethod]
        public void Generate_Category_Specyfic_Product()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                           new Product {ProductID = 1, Name = "Name1", Category = "Cat1"},
                           new Product {ProductID = 2, Name = "Name2", Category = "Cat2"},
                           new Product {ProductID = 3, Name = "Name3", Category = "Cat1"},
                           new Product {ProductID = 4, Name = "Name4", Category = "Cat2"},
                           new Product {ProductID = 5, Name = "Name5", Category = "Cat3"},
            }.AsQueryable());

            ProductController target = new ProductController(mock.Object);
            target.PageSize = 3;

            //Action - thest the product counts for different categories
            int res1 = ((ProductListViewModel)target.List("Cat1").Model).PagingInfo.TotalItems;
            int res2 = ((ProductListViewModel)target.List("Cat2").Model).PagingInfo.TotalItems;
            int res3 = ((ProductListViewModel)target.List("Cat3").Model).PagingInfo.TotalItems;
            int resAll = ((ProductListViewModel)target.List(null).Model).PagingInfo.TotalItems;


            //Assert
            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);

            Assert.AreEqual(resAll, 5);
       }

        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            Cart target = new Cart();

            //Act - add new items
            target.AddItem(p1, 3);
            target.AddItem(p2, 2);
            CartLine[] cartLine = target.Lines.ToArray();

            //Assert
            Assert.AreEqual(cartLine.Length, 2);
            Assert.AreEqual(cartLine[0].Product, p1);
            Assert.AreEqual(cartLine[1].Product, p2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            // Arrange - create a new cart
            Cart target = new Cart();
            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 22);
            CartLine[] results = target.Lines.OrderBy(c => c.Product.ProductID).ToArray();

            // Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Quantity, 23);
            Assert.AreEqual(results[1].Quantity, 1);
        }

        [TestMethod]
        public void Can_Remove_Line() 
        {
            // Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };
            // Arrange - create a new cart
            Cart target = new Cart();
            // Arrange - add some products to the cart
            target.AddItem(p1, 30);
            target.AddItem(p2, 33);
            target.AddItem(p3, 12);

            // Act
            target.RemoveItem(p2);
            // Assert

            Assert.AreEqual(target.Lines.Where(p => p.Product == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            // Arrange - create a new cart
            Cart target = new Cart();
            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();
            // Assert
            Assert.AreEqual(result, 450M);
        }

        [TestMethod]
        public void Can_Clear_Contents() {
            // Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            // Arrange - create a new cart
            Cart target = new Cart();
            // Arrange - add some items
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            // Act - reset the cart
            target.Clear();
            // Assert
            Assert.AreEqual(target.Lines.Count(), 0);
        }


        [TestMethod]
        public void Can_Add_To_Cart()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID=1, Name ="P1", Category="Apples"}
            }.AsQueryable());

            Cart cart = new Cart();

            CartController target = new CartController(mock.Object, null);

            //Act
            target.AddToCart(cart, 1, null);

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void AddingProduct_To_Cart_Goes_To_Cart_Screen()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID=1, Name ="P1", Category="Apples"}
            }.AsQueryable());

            Cart cart = new Cart();

            CartController target = new CartController(mock.Object, null);

            //Act
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            //Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            Cart cart = new Cart();
            CartController terget = new CartController(null, null);

            CartIndexViewModel result = (CartIndexViewModel)terget.Index(cart, "myUrl").ViewData.Model;

            //Assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }
    }
}
